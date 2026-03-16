using CryptoExchange.Net.Objects;
using NUnit.Framework;
using Polymarket.Net.Clients;
using Polymarket.Net.Enums;
using Polymarket.Net.Objects;
using Polymarket.Net.Signing;
using Polymarket.Net.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Polymarket.Net.UnitTests
{
    [TestFixture]
    public class OptimizationTests
    {
        private const string TestPrivateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
        private const string TestAddress = "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266";
        private const string ZeroAddress = "0x0000000000000000000000000000000000000000";

        // ── Signature correctness after all optimizations ──

        [Test]
        public void Signature_ChainId80002_NegRiskFalse_MatchesKnownValue()
        {
            var auth = CreateAuth();
            var parameters = BuildOrderParams("479249096354", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
            var result = auth.GetOrderSignature(parameters, 80002, false).ToLower();
            Assert.That(result, Is.EqualTo("0x302cd9abd0b5fcaa202a344437ec0b6660da984e24ae9ad915a592a90facf5a51bb8a873cd8d270f070217fea1986531d5eec66f1162a81f66e026db653bf7ce1c"));
        }

        [Test]
        public void Signature_ChainId137_NegRiskTrue_MatchesKnownValue()
        {
            var auth = CreateAuth();
            var parameters = BuildOrderParams("1515433236867", "11862165566757345985240476164489718219056735011698825377388402888080786399275", "5000", "5000000", "0", "0", "0", "BUY", 1);
            var result = auth.GetOrderSignature(parameters, 137, true).ToLower();
            Assert.That(result, Is.EqualTo("0x80339932dbe85fda07338f283ba084312addc59c90e7739067be748c12b4922054673e2f8365f5fd891cef19469ec29900d1889d9a674f0bf485f177b6acf14e1b"));
        }

        // ── Signature stability: same inputs produce same output across calls ──

        [Test]
        public void Signature_Repeated_ProducesSameResult()
        {
            var auth = CreateAuth();
            var results = new string[10];
            for (int i = 0; i < 10; i++)
            {
                var parameters = BuildOrderParams("479249096354", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
                results[i] = auth.GetOrderSignature(parameters, 80002, false);
            }

            for (int i = 1; i < 10; i++)
                Assert.That(results[i], Is.EqualTo(results[0]), $"Signature {i} differs from signature 0");
        }

        // ── Different parameters produce different signatures ──

        [Test]
        public void Signature_DifferentSalt_ProducesDifferentResult()
        {
            var auth = CreateAuth();
            var params1 = BuildOrderParams("111111111111", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
            var params2 = BuildOrderParams("222222222222", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
            var sig1 = auth.GetOrderSignature(params1, 80002, false);
            var sig2 = auth.GetOrderSignature(params2, 80002, false);
            Assert.That(sig1, Is.Not.EqualTo(sig2));
        }

        [Test]
        public void Signature_DifferentChainId_ProducesDifferentResult()
        {
            var auth = CreateAuth();
            var params1 = BuildOrderParams("479249096354", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
            var params2 = BuildOrderParams("479249096354", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
            var sig1 = auth.GetOrderSignature(params1, 80002, false);
            var sig2 = auth.GetOrderSignature(params2, 137, false);
            Assert.That(sig1, Is.Not.EqualTo(sig2));
        }

        [Test]
        public void Signature_DifferentNegRisk_ProducesDifferentResult()
        {
            var auth = CreateAuth();
            var params1 = BuildOrderParams("479249096354", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
            var params2 = BuildOrderParams("479249096354", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
            var sig1 = auth.GetOrderSignature(params1, 137, false);
            var sig2 = auth.GetOrderSignature(params2, 137, true);
            Assert.That(sig1, Is.Not.EqualTo(sig2));
        }

        [Test]
        public void Signature_SellSide_ProducesDifferentResult()
        {
            var auth = CreateAuth();
            var params1 = BuildOrderParams("479249096354", "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
            var params2 = BuildOrderParams("479249096354", "1234", "100000000", "50000000", "0", "0", "100", "SELL", 0);
            var sig1 = auth.GetOrderSignature(params1, 80002, false);
            var sig2 = auth.GetOrderSignature(params2, 80002, false);
            Assert.That(sig1, Is.Not.EqualTo(sig2));
        }

        // ── ABI encoder correctness ──

        [Test]
        public void AbiEncodeAddress_Cached_ReturnsSameBytes()
        {
            var result1 = AbiEncoder.AbiValueEncodeAddress(TestAddress);
            var result2 = AbiEncoder.AbiValueEncodeAddress(TestAddress);
            Assert.That(result1, Is.EqualTo(result2));
            Assert.That(result1, Has.Length.EqualTo(32));
        }

        [Test]
        public void AbiEncodeAddress_ZeroAddress_Correct()
        {
            var result = AbiEncoder.AbiValueEncodeAddress(ZeroAddress);
            Assert.That(result, Has.Length.EqualTo(32));
            // Zero address should be all zeros
            for (int i = 0; i < 32; i++)
                Assert.That(result[i], Is.EqualTo(0), $"Byte {i} should be 0");
        }

        [Test]
        public void AbiEncodeAddress_DifferentAddresses_DifferentResults()
        {
            var result1 = AbiEncoder.AbiValueEncodeAddress(TestAddress);
            var result2 = AbiEncoder.AbiValueEncodeAddress(ZeroAddress);
            Assert.That(result1, Is.Not.EqualTo(result2));
        }

        [Test]
        public void AbiEncodeString_Cached_ReturnsSameHash()
        {
            var result1 = AbiEncoder.AbiValueEncodeString("Polymarket CTF Exchange");
            var result2 = AbiEncoder.AbiValueEncodeString("Polymarket CTF Exchange");
            Assert.That(result1, Is.EqualTo(result2));
            Assert.That(result1, Has.Length.EqualTo(32));
        }

        [Test]
        public void AbiEncodeString_DifferentStrings_DifferentHashes()
        {
            var result1 = AbiEncoder.AbiValueEncodeString("Polymarket CTF Exchange");
            var result2 = AbiEncoder.AbiValueEncodeString("Something Else");
            Assert.That(result1, Is.Not.EqualTo(result2));
        }

        [Test]
        public void AbiEncodeBigInteger_SmallUnsigned_CorrectEncoding()
        {
            // 100 as uint256 should be 32 bytes with 100 in the last byte
            var result = AbiEncoder.AbiValueEncodeBigInteger(false, new BigInteger(100));
            Assert.That(result, Has.Length.EqualTo(32));
            Assert.That(result[31], Is.EqualTo(100));
            for (int i = 0; i < 31; i++)
                Assert.That(result[i], Is.EqualTo(0), $"Byte {i} should be 0");
        }

        [Test]
        public void AbiEncodeBigInteger_LargeTokenId_CorrectLength()
        {
            // Real Polymarket tokenId — very large number
            var tokenId = BigInteger.Parse("11862165566757345985240476164489718219056735011698825377388402888080786399275");
            var result = AbiEncoder.AbiValueEncodeBigInteger(false, tokenId);
            Assert.That(result, Has.Length.EqualTo(32));
        }

        [Test]
        public void AbiEncodeInt_UlongMax_CorrectEncoding()
        {
            var result = AbiEncoder.AbiValueEncodeInt(ulong.MaxValue);
            Assert.That(result, Has.Length.EqualTo(32));
            // Last 8 bytes should all be 0xFF
            for (int i = 24; i < 32; i++)
                Assert.That(result[i], Is.EqualTo(0xFF), $"Byte {i} should be 0xFF");
        }

        // ── IsReferenceType correctness (regex optimization) ──

        [Test]
        public void IsReferenceType_PrimitiveTypes_ReturnsFalse()
        {
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("uint256"), Is.False);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("uint8"), Is.False);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("int256"), Is.False);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("bytes32"), Is.False);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("bytes"), Is.False);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("string"), Is.False);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("bool"), Is.False);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("address"), Is.False);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("uint256[]"), Is.False);
        }

        [Test]
        public void IsReferenceType_StructTypes_ReturnsTrue()
        {
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("Order"), Is.True);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("EIP712Domain"), Is.True);
            Assert.That(LightEip712TypedDataEncoder.IsReferenceType("MyStruct"), Is.True);
        }

        // ── Performance: signing should be faster after optimizations ──

        [Test]
        public void Signature_Performance_Under50ms()
        {
            var auth = CreateAuth();

            // Warmup
            var warmupParams = BuildOrderParams("111", "1234", "100", "50", "0", "0", "100", "BUY", 0);
            auth.GetOrderSignature(warmupParams, 137, false);

            // Measure 20 signatures
            var sw = Stopwatch.StartNew();
            const int iterations = 20;
            for (int i = 0; i < iterations; i++)
            {
                var parameters = BuildOrderParams((i + 1000).ToString(), "1234", "100000000", "50000000", "0", "0", "100", "BUY", 0);
                auth.GetOrderSignature(parameters, 137, false);
            }
            sw.Stop();

            var avgMs = sw.ElapsedMilliseconds / (double)iterations;
            TestContext.Out.WriteLine($"Average signing time: {avgMs:F2}ms over {iterations} iterations (total {sw.ElapsedMilliseconds}ms)");

            // After optimizations, each signature should be well under 50ms
            Assert.That(avgMs, Is.LessThan(50), $"Average signing time {avgMs:F2}ms exceeds 50ms threshold");
        }

        // ── Keccak hash correctness with length overload ──

        [Test]
        public void Keccak_LengthOverload_MatchesFullArray()
        {
            var data = new byte[] { 0x19, 0x01, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
            var fullHash = InternalSha3Keccack.CalculateHash(data);

            // Hash with buffer that has extra trailing bytes (simulating GetBuffer() with shorter Length)
            var oversizedBuffer = new byte[64];
            System.Buffer.BlockCopy(data, 0, oversizedBuffer, 0, data.Length);
            var partialHash = InternalSha3Keccack.CalculateHash(oversizedBuffer, data.Length);

            Assert.That(partialHash, Is.EqualTo(fullHash));
        }

        [Test]
        public void Keccak_DifferentLengths_DifferentResults()
        {
            var data = new byte[] { 0x19, 0x01, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
            var hash6 = InternalSha3Keccack.CalculateHash(data, 6);
            var hash8 = InternalSha3Keccack.CalculateHash(data, 8);
            Assert.That(hash6, Is.Not.EqualTo(hash8));
        }

        // ── Public address derivation with cached private key ──

        [Test]
        public void PublicAddress_Consistent_AcrossCalls()
        {
            var auth = CreateAuth();
            var addr1 = auth.GetPublicAddress();
            var addr2 = auth.GetPublicAddress();
            Assert.That(addr1, Is.EqualTo(addr2));
            Assert.That(addr1, Does.StartWith("0x"));
            Assert.That(addr1, Has.Length.EqualTo(42)); // 0x + 40 hex chars
        }

        [Test]
        public void PublicAddress_MatchesExpected()
        {
            var auth = CreateAuth();
            var addr = auth.GetPublicAddress().ToLower();
            Assert.That(addr, Is.EqualTo(TestAddress.ToLower()));
        }

        // ── Helpers ──

        private static PolymarketAuthenticationProvider CreateAuth()
        {
            return new PolymarketAuthenticationProvider(
                new PolymarketCredentials(SignType.EOA, TestPrivateKey, TestAddress));
        }

        private static ParameterCollection BuildOrderParams(
            string salt, string tokenId, string makerAmount, string takerAmount,
            string expiration, string nonce, string feeRateBps, string side, int signatureType)
        {
            return new ParameterCollection
            {
                { "salt", salt },
                { "maker", TestAddress },
                { "signer", TestAddress },
                { "taker", ZeroAddress },
                { "tokenId", tokenId },
                { "makerAmount", makerAmount },
                { "takerAmount", takerAmount },
                { "expiration", expiration },
                { "nonce", nonce },
                { "feeRateBps", feeRateBps },
                { "side", side },
                { "signatureType", signatureType },
            };
        }
    }
}
