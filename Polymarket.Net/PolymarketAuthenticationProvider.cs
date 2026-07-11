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
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Polymarket.Net
{
    internal class PolymarketAuthenticationProvider : AuthenticationProvider<PolymarketCredentials>
    {
        private string? _publicAddress;
        private byte[]? _hmacBytes;
        private byte[]? _privateKeyBytes;

        private const string _l1SignMessage = "This message attests that I control the given wallet";

        private static IStringMessageSerializer _serializer = new SystemTextJsonMessageSerializer(PolymarketPlatform._serializerContext);

        // Pre-built type schemas — identical for every order, no need to reallocate per signature.
        private static readonly Dictionary<string, MemberDescription[]> OrderTypeSchema = new Dictionary<string, MemberDescription[]>
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
                    new MemberDescription { Name = "tokenId", Type = "uint256" },
                    new MemberDescription { Name = "makerAmount", Type = "uint256" },
                    new MemberDescription { Name = "takerAmount", Type = "uint256" },
                    new MemberDescription { Name = "side", Type = "uint8" },
                    new MemberDescription { Name = "signatureType", Type = "uint8" },
                    new MemberDescription { Name = "timestamp", Type = "uint256" },
                    new MemberDescription { Name = "metadata", Type = "bytes32" },
                    new MemberDescription { Name = "builder", Type = "bytes32" },
                }
            }
        };

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

            return new PolymarketInitialQuery<object>("user", _credentials.L2ApiKey, _credentials.L2Secret!, _credentials.L2Pass!);
        }

        private void SignL1Custom(RestRequestConfiguration requestConfig, uint chainId)
        {
            var timestamp = DateTimeConverter.ConvertToSeconds(DateTime.UtcNow);
            requestConfig.GetPositionParameters().TryGetValue("nonce", out var nonce);
            var nonceValue = nonce == null ? 0L : (long)nonce;

            string address;
            string signature;
            if (UseWalletBoundAuth)
            {
                // Deposit-wallet flow: claim the wallet, sign the ClobAuth attestation
                // wrapped in the ERC-7739 TypedDataSign envelope (same shape as the
                // POLY_1271 order signature) so the venue can validate via ERC-1271
                // and bind the api key to the wallet.
                address = _credentials.PolymarketFundingAddress!;
                signature = GetClobAuthSignaturePoly1271(timestamp.Value.ToString(), nonceValue, chainId);
            }
            else
            {
                var typeRaw = GetEncodedClobAuth(timestamp.ToString()!, nonceValue, chainId);
                var msg = LightEip712TypedDataEncoder.EncodeTypedDataRaw(typeRaw);
                var keccakSigned = InternalSha3Keccack.CalculateHash(msg);
                address = GetPublicAddress();
                signature = SignHash(keccakSigned);
            }

            requestConfig.Headers ??= new Dictionary<string, string>();
            requestConfig.Headers.Add("POLY_ADDRESS", address);
            requestConfig.Headers.Add("POLY_SIGNATURE", signature.ToLowerInvariant());
            requestConfig.Headers.Add("POLY_TIMESTAMP", timestamp.Value.ToString());
            requestConfig.Headers.Add("POLY_NONCE", nonce?.ToString() ?? "0");
        }

        private bool UseWalletBoundAuth =>
            _credentials.WalletBoundL1Auth
            && SignatureType == SignType.Poly1271
            && !string.IsNullOrEmpty(_credentials.PolymarketFundingAddress);

        private void SignL2(RestApiClient client, RestRequestConfiguration requestConfig)
        {
            requestConfig.Headers ??= new Dictionary<string, string>();

            var body = string.Empty;
            if (requestConfig.Method == HttpMethod.Post || requestConfig.Method == HttpMethod.Delete)
            {
                body = (requestConfig.BodyParameters == null || requestConfig.BodyParameters.Count == 0) ? string.Empty : SerializeBody(requestConfig.BodyParameters);
                requestConfig.SetBodyContent(body);
            }

            foreach (var header in CreateL2Headers(requestConfig.Method, requestConfig.Path, body))
                requestConfig.Headers.Add(header.Key, header.Value);
        }

        internal Dictionary<string, string> CreateL2Headers(HttpMethod method, string path, string body)
        {
            if (_hmacBytes == null)
                throw new ArgumentException("Layer 2 credentials required");

            var timestamp = DateTimeConverter.ConvertToSeconds(DateTime.UtcNow);
            var signData = timestamp + method.ToString() + path;
            if (method == HttpMethod.Post || method == HttpMethod.Delete)
                signData += body ?? string.Empty;

            string signature;
            using (var hmac = new HMACSHA256(_hmacBytes))
                signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signData)));

            return new Dictionary<string, string>
            {
                // Wallet-bound api keys must claim the wallet on L2 too.
                { "POLY_ADDRESS", UseWalletBoundAuth ? _credentials.PolymarketFundingAddress! : GetPublicAddress() },
                { "POLY_API_KEY", _credentials.L2ApiKey! },
                { "POLY_PASSPHRASE", _credentials.L2Pass! },
                { "POLY_TIMESTAMP", timestamp.Value.ToString() },
                { "POLY_SIGNATURE", signature.Replace('+', '-').Replace('/', '_') }
            };
        }

        internal static string SerializeBody(IDictionary<string, object> parameters)
            => GetSerializedBody(_serializer, parameters);

        public string GetPublicAddress()
        {
            if (_publicAddress != null)
                return _publicAddress;

            var publicKeyBytes = Secp256k1.CreatePublicKey(GetPrivateKeyBytes(), false);

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
            if (SignatureType == SignType.Poly1271)
                return GetOrderSignaturePoly1271(parameters, chainId, negativeRisk);

            var typeRaw = GetTypeDataRawCustom(parameters, chainId, negativeRisk);
            var msg = LightEip712TypedDataEncoder.EncodeTypedDataRaw(typeRaw);
            var orderHashBytes = InternalSha3Keccack.CalculateHash(msg);
            return SignHash(orderHashBytes);
        }

        // ERC-7739 wrapped POLY_1271 signature, mirroring py-clob-client-v2
        // ExchangeOrderBuilderV2._build_poly_1271_order_signature byte-for-byte. The
        // CLOB validates this through the deposit wallet's ERC-1271 isValidSignature
        // hook, which means the inner ECDSA digest must be anchored at the deposit
        // wallet (TypedDataSign domain) and not at the CTF Exchange contract.
        private const string OrderTypeString =
            "Order(uint256 salt,address maker,address signer,uint256 tokenId," +
            "uint256 makerAmount,uint256 takerAmount,uint8 side,uint8 signatureType," +
            "uint256 timestamp,bytes32 metadata,bytes32 builder)";

        private const string SoladyTypeString =
            "TypedDataSign(Order contents,string name,string version,uint256 chainId," +
            "address verifyingContract,bytes32 salt)" + OrderTypeString;

        private const string DomainTypeString =
            "EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)";

        private static readonly byte[] OrderTypeStringBytes = Encoding.UTF8.GetBytes(OrderTypeString);
        private static readonly byte[] OrderTypeHash = InternalSha3Keccack.CalculateHash(OrderTypeStringBytes);
        private static readonly byte[] SoladyTypeHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(SoladyTypeString));
        private static readonly byte[] DomainTypeHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(DomainTypeString));
        private static readonly byte[] DepositWalletNameHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes("DepositWallet"));
        private static readonly byte[] DepositWalletVersionHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes("1"));
        private static readonly byte[] CtfExchangeNameHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes("Polymarket CTF Exchange"));
        private static readonly byte[] CtfExchangeVersionHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes("2"));
        private static readonly byte[] DepositWalletDomainSalt = new byte[32];

        private static readonly Dictionary<string, byte[]> _appDomainSeparatorCache = new(StringComparer.Ordinal);
        private static readonly object _appDomainSeparatorCacheLock = new();

        // --- ERC-7739-wrapped ClobAuth (L1 auth) for POLY_1271 deposit wallets ---
        // Same envelope as the order signature below, with ClobAuth as contents and
        // the ClobAuthDomain (3-field domain: no verifyingContract) as the app domain.
        private const string ClobAuthTypeString =
            "ClobAuth(address address,string timestamp,uint256 nonce,string message)";

        private const string SoladyClobAuthTypeString =
            "TypedDataSign(ClobAuth contents,string name,string version,uint256 chainId," +
            "address verifyingContract,bytes32 salt)" + ClobAuthTypeString;

        private static readonly byte[] ClobAuthTypeStringBytes = Encoding.UTF8.GetBytes(ClobAuthTypeString);
        private static readonly byte[] ClobAuthTypeHash = InternalSha3Keccack.CalculateHash(ClobAuthTypeStringBytes);
        private static readonly byte[] SoladyClobAuthTypeHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(SoladyClobAuthTypeString));
        private static readonly byte[] Domain3TypeHash = InternalSha3Keccack.CalculateHash(
            Encoding.UTF8.GetBytes("EIP712Domain(string name,string version,uint256 chainId)"));
        private static readonly byte[] ClobAuthDomainNameHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes("ClobAuthDomain"));
        private static readonly byte[] ClobAuthDomainVersionHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes("1"));
        private static readonly byte[] L1SignMessageHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(_l1SignMessage));

        internal string GetClobAuthSignaturePoly1271(string timestamp, long nonce, uint chainId)
        {
            var wallet = _credentials.PolymarketFundingAddress!;

            // ClobAuthDomain separator: keccak(domain3TypeHash || name || version || chainId)
            var domBuf = new byte[32 * 4];
            var p = 0;
            Buffer.BlockCopy(Domain3TypeHash, 0, domBuf, p, 32); p += 32;
            Buffer.BlockCopy(ClobAuthDomainNameHash, 0, domBuf, p, 32); p += 32;
            Buffer.BlockCopy(ClobAuthDomainVersionHash, 0, domBuf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeInt(chainId), 0, domBuf, p, 32);
            var clobAuthDomainSep = InternalSha3Keccack.CalculateHash(domBuf);

            // contentsHash = keccak(typeHash || address(wallet) || keccak(ts) || nonce || keccak(msg))
            var cBuf = new byte[32 * 5];
            p = 0;
            Buffer.BlockCopy(ClobAuthTypeHash, 0, cBuf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeAddress(wallet), 0, cBuf, p, 32); p += 32;
            Buffer.BlockCopy(InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(timestamp)), 0, cBuf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeInt((ulong)nonce), 0, cBuf, p, 32); p += 32;
            Buffer.BlockCopy(L1SignMessageHash, 0, cBuf, p, 32);
            var contentsHash = InternalSha3Keccack.CalculateHash(cBuf);

            // TypedDataSign struct hash anchored at the deposit wallet
            var tBuf = new byte[32 * 7];
            p = 0;
            Buffer.BlockCopy(SoladyClobAuthTypeHash, 0, tBuf, p, 32); p += 32;
            Buffer.BlockCopy(contentsHash, 0, tBuf, p, 32); p += 32;
            Buffer.BlockCopy(DepositWalletNameHash, 0, tBuf, p, 32); p += 32;
            Buffer.BlockCopy(DepositWalletVersionHash, 0, tBuf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeInt(chainId), 0, tBuf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeAddress(wallet), 0, tBuf, p, 32); p += 32;
            Buffer.BlockCopy(DepositWalletDomainSalt, 0, tBuf, p, 32);
            var typedDataSignStructHash = InternalSha3Keccack.CalculateHash(tBuf);

            var digestInput = new byte[2 + 32 + 32];
            digestInput[0] = 0x19;
            digestInput[1] = 0x01;
            Buffer.BlockCopy(clobAuthDomainSep, 0, digestInput, 2, 32);
            Buffer.BlockCopy(typedDataSignStructHash, 0, digestInput, 34, 32);
            var digest = InternalSha3Keccack.CalculateHash(digestInput);

            var innerSig = SignHashRaw(digest);

            var wrapped = new byte[65 + 32 + 32 + ClobAuthTypeStringBytes.Length + 2];
            var pos = 0;
            Buffer.BlockCopy(innerSig, 0, wrapped, pos, 65); pos += 65;
            Buffer.BlockCopy(clobAuthDomainSep, 0, wrapped, pos, 32); pos += 32;
            Buffer.BlockCopy(contentsHash, 0, wrapped, pos, 32); pos += 32;
            Buffer.BlockCopy(ClobAuthTypeStringBytes, 0, wrapped, pos, ClobAuthTypeStringBytes.Length); pos += ClobAuthTypeStringBytes.Length;
            wrapped[pos++] = (byte)((ClobAuthTypeStringBytes.Length >> 8) & 0xFF);
            wrapped[pos] = (byte)(ClobAuthTypeStringBytes.Length & 0xFF);

            return "0x" + BytesToHexString(wrapped).ToLowerInvariant();
        }

        public string GetOrderSignaturePoly1271(ParameterCollection order, uint chainId, bool negativeRisk)
        {
            var contractAddr = GetContract(order, chainId, negativeRisk);
            var appDomainSep = GetAppDomainSeparator(chainId, contractAddr);

            var contentsHash = HashOrderContents(order);
            var typedDataSignStructHash = HashTypedDataSignStruct(contentsHash, chainId, (string)order["signer"]);

            // EIP-712 digest: keccak(0x1901 || appDomainSeparator || typedDataSignStructHash)
            var digestInput = new byte[2 + 32 + 32];
            digestInput[0] = 0x19;
            digestInput[1] = 0x01;
            Buffer.BlockCopy(appDomainSep, 0, digestInput, 2, 32);
            Buffer.BlockCopy(typedDataSignStructHash, 0, digestInput, 34, 32);
            var digest = InternalSha3Keccack.CalculateHash(digestInput);

            var innerSig = SignHashRaw(digest);

            // Wrapped layout: innerSig(65) || appDomainSep(32) || contentsHash(32)
            //                 || ORDER_TYPE_STRING bytes || uint16_be(len(ORDER_TYPE_STRING))
            var wrapped = new byte[65 + 32 + 32 + OrderTypeStringBytes.Length + 2];
            var pos = 0;
            Buffer.BlockCopy(innerSig, 0, wrapped, pos, 65); pos += 65;
            Buffer.BlockCopy(appDomainSep, 0, wrapped, pos, 32); pos += 32;
            Buffer.BlockCopy(contentsHash, 0, wrapped, pos, 32); pos += 32;
            Buffer.BlockCopy(OrderTypeStringBytes, 0, wrapped, pos, OrderTypeStringBytes.Length); pos += OrderTypeStringBytes.Length;
            wrapped[pos++] = (byte)((OrderTypeStringBytes.Length >> 8) & 0xFF);
            wrapped[pos] = (byte)(OrderTypeStringBytes.Length & 0xFF);

            return "0x" + BytesToHexString(wrapped).ToLowerInvariant();
        }

        private static byte[] HashOrderContents(ParameterCollection order)
        {
            // abi.encode(ORDER_TYPE_HASH, salt, maker, signer, tokenId, makerAmount,
            //            takerAmount, side, signatureType, timestamp, metadata, builder)
            // = 12 32-byte slots = 384 bytes. (Bug fixed 2026-05-04: was 13 slots
            // and the trailing zero block silently corrupted the keccak input.
            // The /order endpoint rejected with `invalid signature` until this
            // was sized to 12.)
            var buf = new byte[32 * 12];
            var p = 0;

            Buffer.BlockCopy(OrderTypeHash, 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeInt((ulong)order["salt"]), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeAddress((string)order["maker"]), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeAddress((string)order["signer"]), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeBigInteger(false, BigInteger.Parse((string)order["tokenId"])), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeBigInteger(false, BigInteger.Parse((string)order["makerAmount"])), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeBigInteger(false, BigInteger.Parse((string)order["takerAmount"])), 0, buf, p, 32); p += 32;

            var sideByte = (byte)((string)order["side"] == "BUY" ? 0 : 1);
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeInt(sideByte), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeInt((byte)(int)order["signatureType"]), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeBigInteger(false, BigInteger.Parse((string)order["timestamp"])), 0, buf, p, 32); p += 32;

            var metadataBytes = HexToBytesString((string)order["metadata"]);
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeBytes(32, metadataBytes), 0, buf, p, 32); p += 32;
            var builderBytes = HexToBytesString((string)order["builder"]);
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeBytes(32, builderBytes), 0, buf, p, 32);

            return InternalSha3Keccack.CalculateHash(buf);
        }

        private static byte[] HashTypedDataSignStruct(byte[] contentsHash, uint chainId, string signer)
        {
            // abi.encode(SOLADY_TYPE_HASH, contentsHash, DEPOSIT_WALLET_NAME_HASH,
            //            DEPOSIT_WALLET_VERSION_HASH, chainId, signer, DEPOSIT_WALLET_DOMAIN_SALT)
            var buf = new byte[32 * 7];
            var p = 0;
            Buffer.BlockCopy(SoladyTypeHash, 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(contentsHash, 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(DepositWalletNameHash, 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(DepositWalletVersionHash, 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeInt(chainId), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeAddress(signer), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(DepositWalletDomainSalt, 0, buf, p, 32);
            return InternalSha3Keccack.CalculateHash(buf);
        }

        private static byte[] GetAppDomainSeparator(uint chainId, string contractAddr)
        {
            var key = chainId.ToString() + "|" + contractAddr.ToLowerInvariant();
            byte[]? cached;
            lock (_appDomainSeparatorCacheLock)
                _appDomainSeparatorCache.TryGetValue(key, out cached);
            if (cached != null)
                return cached;

            var buf = new byte[32 * 5];
            var p = 0;
            Buffer.BlockCopy(DomainTypeHash, 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(CtfExchangeNameHash, 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(CtfExchangeVersionHash, 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeInt(chainId), 0, buf, p, 32); p += 32;
            Buffer.BlockCopy(AbiEncoder.AbiValueEncodeAddress(contractAddr), 0, buf, p, 32);
            var hash = InternalSha3Keccack.CalculateHash(buf);

            lock (_appDomainSeparatorCacheLock)
                _appDomainSeparatorCache[key] = hash;
            return hash;
        }

        private byte[] SignHashRaw(byte[] hash)
        {
            (var signature, var recover) = Secp256k1.SignRecoverable(hash, GetPrivateKeyBytes());
            var result = new byte[65];
            Buffer.BlockCopy(signature, 0, result, 0, 64);
            result[64] = (byte)(recover + 27);
            return result;
        }

        private byte[] GetPrivateKeyBytes()
        {
            return _privateKeyBytes ??= HexToBytesString(_credentials.L1PrivateKey);
        }

        private string SignHash(byte[] hash)
        {
            (var signature, var recover) = Secp256k1.SignRecoverable(hash, GetPrivateKeyBytes());
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
                    new MemberValue { TypeName = "string", Value = "2" },
                    new MemberValue { TypeName = "uint256", Value = chainId },
                    new MemberValue { TypeName = "address", Value = GetContract(order, chainId, negativeRisk) }
                },
                Message = new MemberValue[]
                {
                    new MemberValue { TypeName = "uint256", Value = order["salt"].ToString()! },
                    new MemberValue { TypeName = "address", Value = order["maker"]},
                    new MemberValue { TypeName = "address", Value = order["signer"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["tokenId"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["makerAmount"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["takerAmount"]},
                    new MemberValue { TypeName = "uint8", Value = (byte)((string)order["side"] == "BUY" ? 0 : 1)},
                    new MemberValue { TypeName = "uint8", Value = (byte)(int)order["signatureType"]},
                    new MemberValue { TypeName = "uint256", Value = (string)order["timestamp"]},
                    new MemberValue { TypeName = "bytes32", Value = HexToBytesString((string)order["metadata"])},
                    new MemberValue { TypeName = "bytes32", Value = HexToBytesString((string)order["builder"])}
                },
                Types = OrderTypeSchema
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
