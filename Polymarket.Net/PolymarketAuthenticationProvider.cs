using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Polymarket.Net.Enums;
using Polymarket.Net.Objects;
using Polymarket.Net.Objects.Options;
using Polymarket.Net.Objects.Sockets;
using Polymarket.Net.Signing;
using Polymarket.Net.Utils;
using Secp256k1Net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Polymarket.Net
{
    internal class PolymarketAuthenticationProvider : AuthenticationProvider<PolymarketCredentials>
    {
        private string? _publicAddress;
        private byte[]? _hmacBytes;

        private const string _l1SignMessage = "This message attests that I control the given wallet";

        private static IStringMessageSerializer _serializer = new SystemTextJsonMessageSerializer(PolymarketPlatform._serializerContext);

        public string PublicAddress => GetPublicAddress();
        public SignType SignatureType => _credentials.SignatureType;
        public string? PolymarketFundingAddress => _credentials.PolymarketFundingAddress;
        public override ApiCredentialsType[] SupportedCredentialTypes => [ApiCredentialsType.Hmac];

        public PolymarketAuthenticationProvider(PolymarketCredentials credentials) : base(credentials)
        {
            if (!string.IsNullOrEmpty(_credentials.L2Secret))
            {
                try
                {
                    _hmacBytes = Convert.FromBase64String(_credentials.L2Secret!.Replace('-', '+').Replace('_', '/'));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Provided secret invalid, not in base64 format", ex);
                }
            }
        }

        public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig)
        {
            if (!requestConfig.Authenticated)
                return;

            if ((requestConfig.Path.Equals("/auth/api-key") && requestConfig.Method == HttpMethod.Post)
                || (requestConfig.Path.Equals("/auth/derive-api-key") && requestConfig.Method == HttpMethod.Get))
            {
                // L1 authentication
                SignL1Custom(requestConfig, ((PolymarketRestOptions)apiClient.ClientOptions).Environment.ChainId);
            }
            else
            {
                // L2 authentication
                SignL2(apiClient, requestConfig);
            }
        }

        public override Query? GetAuthenticationQuery(SocketApiClient apiClient, SocketConnection connection, Dictionary<string, object?>? context = null)
        {
            if (_credentials.L2ApiKey == null)
                throw new InvalidOperationException("Layer 2 credentials required");

            return new PolymarketInitialQuery<object>("USER", _credentials.L2ApiKey, _credentials.L2Secret!, _credentials.L2Pass!);
        }

        private void SignL1Custom(RestRequestConfiguration requestConfig, uint chainId)
        {
            var timestamp = DateTimeConverter.ConvertToSeconds(DateTime.UtcNow);
            requestConfig.GetPositionParameters().TryGetValue("nonce", out var nonce);

            var typeRaw = GetEncodedClobAuth(timestamp.ToString()!, nonce == null ? 0 : (long)nonce, chainId);
            var msg = LightEip712TypedDataEncoder.EncodeTypedDataRaw(typeRaw);
            var keccakSigned = InternalSha3Keccack.CalculateHash(msg);

            var signature = SignHash(keccakSigned);
            requestConfig.Headers ??= new Dictionary<string, string>();
            requestConfig.Headers.Add("POLY_ADDRESS", GetPublicAddress());
            requestConfig.Headers.Add("POLY_SIGNATURE", signature.ToLowerInvariant());
            requestConfig.Headers.Add("POLY_TIMESTAMP", timestamp.Value.ToString());
            requestConfig.Headers.Add("POLY_NONCE", nonce?.ToString() ?? "0");
        }

        private void SignL2(RestApiClient client, RestRequestConfiguration requestConfig)
        {
            if (_hmacBytes == null)
                throw new ArgumentException("Layer 2 credentials required");

            var timestamp = DateTimeConverter.ConvertToSeconds(DateTime.UtcNow);
            requestConfig.Headers ??= new Dictionary<string, string>();
            requestConfig.Headers.Add("POLY_ADDRESS", GetPublicAddress());
            requestConfig.Headers.Add("POLY_API_KEY", _credentials.L2ApiKey!);
            requestConfig.Headers.Add("POLY_PASSPHRASE", _credentials.L2Pass!);
            requestConfig.Headers.Add("POLY_TIMESTAMP", timestamp.Value.ToString());

            var signData = timestamp + requestConfig.Method.ToString() + requestConfig.Path;
            if (requestConfig.Method == HttpMethod.Post || requestConfig.Method == HttpMethod.Delete)
            {
                var body = (requestConfig.BodyParameters == null || requestConfig.BodyParameters.Count == 0) ? string.Empty : GetSerializedBody(_serializer, requestConfig.BodyParameters);
                signData += body;
                requestConfig.SetBodyContent(body);
            }

            string signature;
            using (var hmac = new HMACSHA256(_hmacBytes))
                signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signData)));

            requestConfig.Headers.Add("POLY_SIGNATURE", signature.Replace('+', '-').Replace('/', '_'));

            var options = (PolymarketRestOptions)client.ClientOptions;
            if (string.IsNullOrEmpty(options.BuilderApiKey)
                || string.IsNullOrEmpty(options.BuilderSecret)
                || string.IsNullOrEmpty(options.BuilderPass))
            {
                return;
            }

            // Builder headers
            var secret = Convert.FromBase64String(options.BuilderSecret!.Replace('-', '+').Replace('_', '/'));

            using var encryptor = new HMACSHA256(secret);
            var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(signData));
            var builderSignature = BytesToBase64String(resultBytes);
            builderSignature = builderSignature.Replace('+', '-').Replace('/', '_');

            requestConfig.Headers.Add("POLY_BUILDER_API_KEY", options.BuilderApiKey!);
            requestConfig.Headers.Add("POLY_BUILDER_PASSPHRASE", options.BuilderPass!);
            requestConfig.Headers.Add("POLY_BUILDER_SIGNATURE", builderSignature);
            requestConfig.Headers.Add("POLY_BUILDER_TIMESTAMP", timestamp.Value.ToString());
        }

        public string GetPublicAddress()
        {
            if (_publicAddress != null)
                return _publicAddress;

            var publicKeyBytes = Secp256k1.CreatePublicKey(HexToBytesString(_credentials.L1PrivateKey), false);

            var withoutPrefix = new byte[64];
            Array.Copy(publicKeyBytes, 1, withoutPrefix, 0, 64);

            var hash = InternalSha3Keccack.CalculateHash(withoutPrefix);
            var pubAddress = new byte[20];
            Array.Copy(hash, hash.Length - 20, pubAddress, 0, 20);

            _publicAddress = "0x" + BytesToHexString(pubAddress); // Public address
            return _publicAddress;
        }

        public string GetOrderSignature(ParameterCollection parameters, uint chainId, bool negativeRisk)
        {
            var typeRaw = GetTypeDataRawCustom(parameters, chainId, negativeRisk);
            var msg = LightEip712TypedDataEncoder.EncodeTypedDataRaw(typeRaw);
            var orderHashBytes = InternalSha3Keccack.CalculateHash(msg);
            return SignHash(orderHashBytes);
        }

        private string SignHash(byte[] hash)
        {
            (var signature, var recover) = Secp256k1.SignRecoverable(hash, HexToBytesString(_credentials.L1PrivateKey));
            var hexCompactR = BytesToHexString(new ArraySegment<byte>(signature, 0, 32));
            var hexCompactS = BytesToHexString(new ArraySegment<byte>(signature, 32, 32));
            var hexCompactV = BytesToHexString([(byte)(recover + 27)]);

            var result = "0x" + hexCompactR.PadLeft(64, '0') +
                   hexCompactS.PadLeft(64, '0') +
                   hexCompactV;
            return result;
        }

        private string GetContract(ParameterCollection order, uint chainId, bool negativeRisk)
        {
            if (chainId == 137)            
                return negativeRisk ? PolymarketContractsConfig.PolygonNegRiskConfig.Exchange : PolymarketContractsConfig.PolygonConfig.Exchange;            
            else if (chainId == 80002)
                return negativeRisk ? PolymarketContractsConfig.AmoyNegRiskConfig.Exchange : PolymarketContractsConfig.AmoyConfig.Exchange;
            else
                throw new InvalidOperationException("Unknown chainId: " + chainId);
        }

        private TypedDataRaw GetTypeDataRawCustom(ParameterCollection order, uint chainId, bool negativeRisk)
        {
            return new TypedDataRaw
            {
                PrimaryType = "Order",
                DomainRawValues = new MemberValue[]
                {
                    new MemberValue { TypeName = "string", Value = "Polymarket CTF Exchange" },
                    new MemberValue { TypeName = "string", Value = "1" },
                    new MemberValue { TypeName = "uint256", Value = chainId },
                    new MemberValue { TypeName = "address", Value = GetContract(order, chainId, negativeRisk) }
                },
                Message = new MemberValue[]
                {
                    new MemberValue { TypeName = "uint256", Value = order["salt"].ToString()! },
                    new MemberValue { TypeName = "address", Value = order["maker"]},
                    new MemberValue { TypeName = "address", Value = order["signer"]},
                    new MemberValue { TypeName = "address", Value = order["taker"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["tokenId"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["makerAmount"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["takerAmount"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["expiration"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["nonce"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["feeRateBps"]},
                    new MemberValue { TypeName = "uint8", Value = (byte)((string)order["side"] == "BUY" ? 0 : 1)},
                    new MemberValue { TypeName = "uint8", Value = (byte)(int)order["signatureType"]}
                },
                Types = new Dictionary<string, MemberDescription[]>
                {
                    { "EIP712Domain",
                        new MemberDescription[]
                        {
                            new MemberDescription { Name = "name", Type = "string" },
                            new MemberDescription { Name = "version", Type = "string" },
                            new MemberDescription { Name = "chainId", Type = "uint256" },
                            new MemberDescription { Name = "verifyingContract", Type = "address" }
                        }
                    },
                    { "Order",
                        new MemberDescription[]
                        {
                            new MemberDescription { Name = "salt", Type = "uint256" },
                            new MemberDescription { Name = "maker", Type = "address" },
                            new MemberDescription { Name = "signer", Type = "address" },
                            new MemberDescription { Name = "taker", Type = "address" },
                            new MemberDescription { Name = "tokenId", Type = "uint256" },
                            new MemberDescription { Name = "makerAmount", Type = "uint256" },
                            new MemberDescription { Name = "takerAmount", Type = "uint256" },
                            new MemberDescription { Name = "expiration", Type = "uint256" },
                            new MemberDescription { Name = "nonce", Type = "uint256" },
                            new MemberDescription { Name = "feeRateBps", Type = "uint256" },
                            new MemberDescription { Name = "side", Type = "uint8" },
                            new MemberDescription { Name = "signatureType", Type = "uint8" },
                        }
                    }
                }
            };
        }

        public TypedDataRaw GetEncodedClobAuth(string timestamp, long nonce, uint chainId)
        {
            return new TypedDataRaw
            {
                PrimaryType = "ClobAuth",
                DomainRawValues = new MemberValue[]
                {
                    new MemberValue { TypeName = "string", Value = "ClobAuthDomain" },
                    new MemberValue { TypeName = "string", Value = "1" },
                    new MemberValue { TypeName = "uint256", Value = chainId },
                },
                Message = new MemberValue[]
                {
                    new MemberValue { TypeName = "address", Value = PublicAddress },
                    new MemberValue { TypeName = "string", Value = timestamp },
                    new MemberValue { TypeName = "uint256", Value = nonce },
                    new MemberValue { TypeName = "string", Value = _l1SignMessage }
                },
                Types = new Dictionary<string, MemberDescription[]>
                {
                    { "EIP712Domain",
                        new MemberDescription[]
                        {
                            new MemberDescription { Name = "name", Type = "string" },
                            new MemberDescription { Name = "version", Type = "string" },
                            new MemberDescription { Name = "chainId", Type = "uint256" }
                        }
                    },
                    { "ClobAuth",
                        new MemberDescription[]
                        {
                            new MemberDescription { Name = "address", Type = "address" },
                            new MemberDescription { Name = "timestamp", Type = "string" },
                            new MemberDescription { Name = "nonce", Type = "uint256" },
                            new MemberDescription { Name = "message", Type = "string" }
                        }
                    }
                }
            };
        }
    }
}
