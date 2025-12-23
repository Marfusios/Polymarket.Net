using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Polymarket.Net.Objects;
using Polymarket.Net.Signing;
using Polymarket.Net.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;

namespace Polymarket.Net
{
    internal class PolymarketAuthenticationProvider : AuthenticationProvider<PolymarketCredentials>
    {
        private bool _updatedBaseBytes;

        private const string _l1SignMessage = "This message attests that I control the given wallet";

        private static IStringMessageSerializer _serializer = new SystemTextJsonMessageSerializer(PolymarketExchange._serializerContext);

        public override ApiCredentialsType[] SupportedCredentialTypes => [ApiCredentialsType.Hmac];

        public PolymarketAuthenticationProvider(PolymarketCredentials credentials) : base(credentials)
        {
#warning some way to better differentiate public/private key creds and apikey/secret/pass creds. Maybe just a 
        }


        public override void ProcessRequest(RestApiClient apiClient, RestRequestConfiguration requestConfig)
        {
            if (!requestConfig.Authenticated)
                return;

            if ((requestConfig.Path.Equals("/auth/api-key") && requestConfig.Method == HttpMethod.Post)
                || (requestConfig.Path.Equals("/auth/derive-api-key") && requestConfig.Method == HttpMethod.Get))
            {
                // L1 authentication
                // SignL1Neth(requestConfig);
                SignL1Custom(requestConfig);
            }
            else
            {
                // L2 authentication
                SignL2(requestConfig);
            }
        }

        private void SignL1Custom(RestRequestConfiguration requestConfig)
        {
            var timestamp = DateTimeConverter.ConvertToSeconds(DateTime.UtcNow);
            requestConfig.GetPositionParameters().TryGetValue("nonce", out var nonce);

            var msg = EncodeEip721(timestamp.ToString()!, nonce == null ? 0 : (long)nonce);
            var keccakSigned = BytesToHexString(InternalSha3Keccack.CalculateHash(msg));

            var signature = SignRequest(keccakSigned, _credentials.Secret);
            requestConfig.Headers ??= new Dictionary<string, string>();
            requestConfig.Headers.Add("POLY_ADDRESS", ApiKey);
            requestConfig.Headers.Add("POLY_SIGNATURE", signature.ToLowerInvariant());
            requestConfig.Headers.Add("POLY_TIMESTAMP", timestamp.Value.ToString());
            requestConfig.Headers.Add("POLY_NONCE", nonce?.ToString() ?? "0");
        }

        //private void SignL1Neth(RestRequestConfiguration requestConfig)
        //{
        //    var timestamp = DateTimeConverter.ConvertToSeconds(DateTime.UtcNow);
        //    requestConfig.GetPositionParameters().TryGetValue("nonce", out var nonce);

        //    var typedData = new UsdClassTransfer
        //    {
        //        Address = ApiKey,
        //        Message = _l1SignMessage,
        //        Nonce = nonce == null ? 0 : (long)nonce,
        //        Timestamp = timestamp.Value.ToString()
        //    };
        //    var msg = EncodeEip721Neth(typedData, 137, "ClobAuth");
        //    var keccakSigned = BytesToHexString(InternalSha3Keccack.CalculateHash(msg));

        //    var messageBytes = ConvertHexStringToByteArray(keccakSigned);
        //    var sign = new MessageSigner().SignAndCalculateV(messageBytes, new EthECKey(_credentials.Secret));
        //    var signature = sign.CreateStringSignature();

        //    requestConfig.Headers ??= new Dictionary<string, string>();
        //    requestConfig.Headers.Add("POLY_ADDRESS", ApiKey);
        //    requestConfig.Headers.Add("POLY_SIGNATURE", signature);
        //    requestConfig.Headers.Add("POLY_TIMESTAMP", timestamp.Value.ToString());
        //    requestConfig.Headers.Add("POLY_NONCE", nonce?.ToString() ?? "0");
        //}

        private void SignL2(RestRequestConfiguration requestConfig)
        {
            if (!_updatedBaseBytes)
            {
                _sBytes = Convert.FromBase64String(_credentials.Secret.Replace('-', '+').Replace('_', '/'));
                _updatedBaseBytes = true;
            }

            requestConfig.Headers ??= new Dictionary<string, string>();
            requestConfig.Headers.Add("POLY_ADDRESS", _credentials.Address);
            requestConfig.Headers.Add("POLY_API_KEY", ApiKey);
            requestConfig.Headers.Add("POLY_PASSPHRASE", Pass!);

            var timestamp = DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow);
            requestConfig.Headers.Add("POLY_TIMESTAMP", timestamp.Value.ToString());

            var signData = timestamp + requestConfig.Method.ToString() + requestConfig.Path;
            if (requestConfig.Method == HttpMethod.Post || requestConfig.Method == HttpMethod.Delete)
            {
                var body = (requestConfig.BodyParameters == null || requestConfig.BodyParameters.Count == 0) ? string.Empty : GetSerializedBody(_serializer, requestConfig.BodyParameters);
                signData += body;
                requestConfig.SetBodyContent(body);
            }

            var signature = SignHMACSHA256(signData, SignOutputType.Base64);
            signature = signature.Replace('+', '-').Replace('/', '_');
            requestConfig.Headers.Add("POLY_SIGNATURE", signature);

#warning signature type
            if (requestConfig.ParameterPosition == HttpMethodParameterPosition.InUri)
            {
                requestConfig.QueryParameters ??= new Dictionary<string, object>();
                requestConfig.QueryParameters.Add("signature_type", 0);
            }
            else
            {
                requestConfig.BodyParameters ??= new Dictionary<string, object>();
                requestConfig.BodyParameters.Add("signature_type", 0);
            }
        }

        //public byte[] EncodeEip721Neth(
        //    object msg,
        //    int chainId,
        //    string primaryType)
        //{
        //    var typeDef = GetMessageTypedDefinition(chainId, msg.GetType(), primaryType);

        //    var signer = new Eip712TypedDataSigner();
        //    var encodedData = signer.EncodeTypedData((UsdClassTransfer)msg, typeDef);
        //    return encodedData;
        //}

        //public static TypedData<DomainWithNameVersionAndChainId> GetMessageTypedDefinition(int chainId, Type messageType, string primaryType)
        //{
        //    return new TypedData<DomainWithNameVersionAndChainId>
        //    {
        //        Domain = new DomainWithNameVersionAndChainId
        //        {
        //            Name = "ClobAuthDomain",
        //            Version = "1",
        //            ChainId = chainId
        //        },
        //        Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(DomainWithNameVersionAndChainId), messageType),
        //        PrimaryType = primaryType,
        //    };
        //}


        //[Struct("ClobAuth")]
        //public class UsdClassTransfer
        //{
        //    [Parameter("address", "address", 1)]
        //    public string Address { get; set; }
        //    [Parameter("string", "timestamp", 2)]
        //    public string Timestamp { get; set; }
        //    [Parameter("uint256", "nonce", 3)]
        //    public long Nonce { get; set; }
        //    [Parameter("string", "message", 4)]
        //    public string Message { get; set; }
        //}


        public byte[] EncodeEip721(string timestamp, long nonce)
        {
            var typeRaw = new TypedDataRaw
            {
                PrimaryType = "ClobAuth",
                DomainRawValues = new MemberValue[]
                {
                    new MemberValue { TypeName = "string", Value = "ClobAuthDomain" },
                    new MemberValue { TypeName = "string", Value = "1" },
                    new MemberValue { TypeName = "uint256", Value = 137 },
                },
                Message = new MemberValue[]
                {
                    new MemberValue { TypeName = "address", Value = _credentials.Key },
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

            return LightEip712TypedDataEncoder.EncodeTypedDataRaw(typeRaw);
        }

        public static string SignRequest(string request, string secret)
        {
            var messageBytes = ConvertHexStringToByteArray(request);
            var bSecret = secret.HexToByteArray();

            ECParameters eCParameters = new ECParameters()
            {
                D = FixSize(bSecret, 32),
                Curve = ECCurve.CreateFromFriendlyName("secp256k1"),
                Q =
                {
                    X = null,
                    Y = null
                }
            };

            using (ECDsa dsa = ECDsa.Create(eCParameters))
            {
                var s = dsa.SignHash(messageBytes);
                var rs = NormalizeSignature(s);
                var parameters = dsa.ExportParameters(false);

                var c = new byte[33];

                rs.r.AsEnumerable().Reverse().ToArray().CopyTo(c, 0);
                BigInteger rValue = new BigInteger(c);
                c = new byte[33];
                rs.s.AsEnumerable().Reverse().ToArray().CopyTo(c, 0);
                BigInteger sValue = new BigInteger(c);

                var v = RecoverFromSignature(rValue, sValue, messageBytes, parameters.Q.X!, parameters.Q.Y!);

                return "0x" + BytesToHexString(rs.r).PadLeft(64, '0') + BytesToHexString(rs.s) +  BytesToHexString([Convert.ToByte(v)]);
            }
        }

        private static byte[] FixSize(byte[] input, int expectedSize)
        {
            if (input.Length == expectedSize)
                return input;

            byte[] tmp;
            if (input.Length < expectedSize)
            {
                tmp = new byte[expectedSize];
                Buffer.BlockCopy(input, 0, tmp, expectedSize - input.Length, input.Length);
                return tmp;
            }

            if (input.Length > expectedSize + 1 || input[0] != 0)
                throw new InvalidOperationException();

            tmp = new byte[expectedSize];
            Buffer.BlockCopy(input, 1, tmp, 0, expectedSize);
            return tmp;
        }

        public static (byte[] r, byte[] s, bool flip) NormalizeSignature(byte[] signature)
        {
            // Ensure the signature is in the correct format (r, s)
            if (signature.Length != 64)
                throw new ArgumentException("Invalid signature length.");

            byte[] r = new byte[32];
            byte[] s = new byte[32];
            Array.Copy(signature, 0, r, 0, 32);
            Array.Copy(signature, 32, s, 0, 32);

            // Normalize the 's' value to be in the lower half of the curve order
            byte[] c = new byte[33];
            s.AsEnumerable().Reverse().ToArray().CopyTo(c, 0);
            BigInteger sValue = new BigInteger(c);
            byte[] normalizedS;
            var flip = false;
            if (sValue > Secp256k1PointCalculator._halfN)
            {
                sValue = Secp256k1PointCalculator._n - sValue;
                flip = true;
                normalizedS = sValue.ToByteArray().AsEnumerable().Reverse().ToArray();
                if (normalizedS.Length < 32)
                {
                    byte[] paddedS = new byte[32];
                    Array.Copy(normalizedS, 0, paddedS, 32 - normalizedS.Length, normalizedS.Length);
                    normalizedS = paddedS;
                }
            }
            else
            {
                normalizedS = s;
            }

            return (r, normalizedS, flip);
        }

        private static int RecoverFromSignature(BigInteger r, BigInteger s, byte[] message, byte[] publicKeyX, byte[] publicKeyY)
        {
            if (r < 0)
                throw new ArgumentException("r should be positive");
            if (s < 0)
                throw new ArgumentException("s should be positive");
            if (message == null)
                throw new ArgumentNullException("message");

            byte[] c = new byte[33];
            publicKeyX.AsEnumerable().Reverse().ToArray().CopyTo(c, 0);
            BigInteger publicKeyXValue = new BigInteger(c);

            c = new byte[33];
            publicKeyY.AsEnumerable().Reverse().ToArray().CopyTo(c, 0);
            BigInteger publicKeyYValue = new BigInteger(c);

            // Compute e from M using Steps 2 and 3 of ECDSA signature verification.
            c = new byte[33];
            message.AsEnumerable().Reverse().ToArray().CopyTo(c, 0);
            var e = new BigInteger(c);

            var eInv = (-e) % Secp256k1PointCalculator._n;
            if (eInv < 0)
                eInv += Secp256k1PointCalculator._n;

            var rInv = BigInteger.ModPow(r, Secp256k1PointCalculator._n - 2, Secp256k1PointCalculator._n);
            var srInv = (rInv * s) % Secp256k1PointCalculator._n;
            var eInvrInv = (rInv * eInv) % Secp256k1PointCalculator._n;

            var recId = -1;

            for (var i = 0; i < 4; i++)
            {
                recId = i;
                var intAdd = recId / 2;
                var x = r + (intAdd * Secp256k1PointCalculator._n);

                if (x < Secp256k1ZCalculator._q)
                {
                    var R = Secp256k1PointCalculator.DecompressPointSecp256k1(x, (recId & 1));
                    var tx = R.X.ToString("x");
                    var ty = R.Y.ToString("x");
                    var b = tx == ty;
                    if (R.MultiplyByN().IsInfinity())
                    {
                        var q = Secp256k1PointCalculator.SumOfTwoMultiplies(new Secp256k1PointPreCompCache(), Secp256k1PointCalculator._g, eInvrInv, R, srInv);
                        q = q.Normalize();
                        if (q.X == publicKeyXValue && q.Y == publicKeyYValue)
                        {
                            recId = i;
                            break;
                        }
                    }
                }
            }

            if (recId == -1)
                throw new Exception("Could not construct a recoverable key. This should never happen.");

            return recId;
        }

        private static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.StartsWith("0x"))
                hexString = hexString.Substring(2);

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                string hexSubstring = hexString.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(hexSubstring, 16);
            }

            return bytes;
        }
    }
}
