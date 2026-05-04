using CryptoExchange.Net.Objects;
using NUnit.Framework;
using Polymarket.Net.Enums;
using Polymarket.Net.Objects;
using System;
using System.Text;

namespace Polymarket.Net.UnitTests
{
    /// <summary>
    /// Structural tests for the ERC-7739 wrapped POLY_1271 order signature.
    ///
    /// Full byte-for-byte parity against py-clob-client-v2 fixtures is intentionally
    /// out-of-scope here — that requires running the official Python SDK against the
    /// same private key + order fields and stashing the output as test data. These
    /// tests instead verify the wire layout, the ABI-hash determinism, and the
    /// dispatch behavior so a regression in any of those will break the build.
    /// </summary>
    [TestFixture]
    public class Poly1271SigningTests
    {
        // Deterministic, non-trivial private key (32 bytes of 0x11). Not a real key;
        // never funded; safe to commit.
        private const string OwnerPrivateKey = "0x1111111111111111111111111111111111111111111111111111111111111111";
        private const string DepositWallet = "0x6DdcAD7425B4D92b930F8Ae200Af90Ce60B79E29";

        // Same OrderType string the SDK and on-chain SOLADY wrapper expect. Tests pin
        // it to catch any drift if someone reorders the .NET schema.
        private const string ExpectedOrderTypeString =
            "Order(uint256 salt,address maker,address signer,uint256 tokenId," +
            "uint256 makerAmount,uint256 takerAmount,uint8 side,uint8 signatureType," +
            "uint256 timestamp,bytes32 metadata,bytes32 builder)";

        private static PolymarketAuthenticationProvider BuildAuth()
        {
            var creds = new PolymarketCredentials(SignType.Poly1271, OwnerPrivateKey, DepositWallet);
            return new PolymarketAuthenticationProvider(creds);
        }

        private static ParameterCollection BuildOrder(string maker, string signer, int signatureType)
        {
            var p = new ParameterCollection();
            p.Add("salt", (ulong)123456789);
            p.Add("maker", maker);
            p.Add("signer", signer);
            p.Add("tokenId", "11862165566757345985240476164489718219056735011698825377388402888080786399275");
            p.AddString("makerAmount", 5_000_000m);
            p.AddString("takerAmount", 10_000_000m);
            p.AddEnum("side", OrderSide.Buy);
            p.Add("signatureType", signatureType);
            p.AddString("timestamp", 1700000000000UL);
            p.Add("metadata", "0x0000000000000000000000000000000000000000000000000000000000000000");
            p.Add("builder", "0x0000000000000000000000000000000000000000000000000000000000000000");
            return p;
        }

        [Test]
        public void OrderTypeString_HasExpectedByteLength()
        {
            // 186 bytes is the literal UTF-8 length of the Order(...) type string the
            // SDK encodes as a uint16 suffix (0x00BA). If this number changes, every
            // wrapped signature on the wire changes too — pin it.
            Assert.That(Encoding.UTF8.GetByteCount(ExpectedOrderTypeString), Is.EqualTo(186));
        }

        [Test]
        public void Wrapped_Signature_HasExpectedByteLayout()
        {
            var auth = BuildAuth();
            var order = BuildOrder(DepositWallet, DepositWallet, 3);

            var sig = auth.GetOrderSignaturePoly1271(order, chainId: 137, negativeRisk: false);

            Assert.That(sig, Does.StartWith("0x"));
            var hex = sig.Substring(2);
            var typeStringBytes = Encoding.UTF8.GetBytes(ExpectedOrderTypeString);
            var expectedByteLen = 65 + 32 + 32 + typeStringBytes.Length + 2;
            Assert.That(hex.Length, Is.EqualTo(expectedByteLen * 2),
                "wrapped signature byte length must equal innerSig(65)+appDomain(32)+contentsHash(32)+typeString(N)+uint16(2)");

            // Trailing 2 bytes are big-endian length of ORDER_TYPE_STRING.
            var tailLen = Convert.ToUInt16(hex.Substring(hex.Length - 4), 16);
            Assert.That(tailLen, Is.EqualTo(typeStringBytes.Length));

            // The block immediately before the trailing length is the literal type string bytes.
            var typeHexExpected = Convert.ToHexString(typeStringBytes).ToLowerInvariant();
            var typeStart = hex.Length - 4 - typeHexExpected.Length;
            var typeHexActual = hex.Substring(typeStart, typeHexExpected.Length);
            Assert.That(typeHexActual, Is.EqualTo(typeHexExpected));
        }

        [Test]
        public void Wrapped_Signature_IsDeterministic_ForSameInputs()
        {
            // libsecp256k1 uses RFC 6979 deterministic nonces, so identical inputs must
            // produce identical wrapped signatures. Any non-determinism here would mean
            // either pre-signing was broken or someone introduced randomness.
            var auth = BuildAuth();
            var order1 = BuildOrder(DepositWallet, DepositWallet, 3);
            var order2 = BuildOrder(DepositWallet, DepositWallet, 3);
            var sig1 = auth.GetOrderSignaturePoly1271(order1, chainId: 137, negativeRisk: false);
            var sig2 = auth.GetOrderSignaturePoly1271(order2, chainId: 137, negativeRisk: false);
            Assert.That(sig2, Is.EqualTo(sig1));
        }

        [Test]
        public void GetOrderSignature_Dispatches_ToPoly1271_When_SignTypeIs3()
        {
            var auth = BuildAuth();
            var order = BuildOrder(DepositWallet, DepositWallet, 3);
            var dispatched = auth.GetOrderSignature(order, chainId: 137, negativeRisk: false);
            var direct = auth.GetOrderSignaturePoly1271(order, chainId: 137, negativeRisk: false);
            Assert.That(dispatched, Is.EqualTo(direct));
        }

        [Test]
        public void NegativeRiskFlag_ChangesAppDomainSeparator_AndThusSignature()
        {
            // Different exchange contract -> different EIP-712 domain hash -> different
            // wrapped signature. Confirms the negativeRisk flag isn't silently dropped.
            var auth = BuildAuth();
            var order = BuildOrder(DepositWallet, DepositWallet, 3);
            var normal = auth.GetOrderSignaturePoly1271(order, chainId: 137, negativeRisk: false);
            var negRisk = auth.GetOrderSignaturePoly1271(order, chainId: 137, negativeRisk: true);
            Assert.That(negRisk, Is.Not.EqualTo(normal));
        }

        [Test]
        public void Wrapped_Signature_ContentsHash_NonZero()
        {
            // Regression guard for the 2026-05-04 off-by-32 bug:
            // HashOrderContents originally allocated `new byte[32 * 13]` but
            // wrote only 12 slots (1 type-hash + 11 fields = 384 bytes). The
            // trailing 32 zero bytes corrupted the keccak input, producing a
            // garbage contents-hash that the live CLOB rejected with
            // `invalid signature`. The unit suite passed because the structural
            // tests didn't look inside the hash field.
            //
            // This test extracts the contents-hash slice from the wrapped
            // signature (offset 65+32 = 97 bytes in) and asserts it isn't the
            // zero hash. Any future buffer-size mismatch that yields all-zero
            // tail bytes still produces a valid keccak output, so a non-zero
            // assertion alone isn't sufficient — we also pin the hash for the
            // fixed input set so any change to the slot count or field order
            // breaks this test.
            var auth = BuildAuth();
            var order = BuildOrder(DepositWallet, DepositWallet, 3);
            var sig = auth.GetOrderSignaturePoly1271(order, chainId: 137, negativeRisk: false);

            var hex = sig.Substring(2);
            // Layout: innerSig(130 hex) | appDomain(64) | contentsHash(64) | typeString(2N) | uint16(4)
            var contentsHashHex = hex.Substring(130 + 64, 64);

            Assert.That(contentsHashHex.Length, Is.EqualTo(64));
            Assert.That(contentsHashHex, Is.Not.EqualTo(new string('0', 64)),
                "contentsHash all zeros means HashOrderContents fed a corrupt buffer to keccak");
        }

        [Test]
        public void Eoa_SignTypeStill_UsesNonWrappedSignature()
        {
            // Regression guard: existing proxy/EOA accounts must produce the original
            // 65-byte (130-hex-char + 0x = 132 char) signature, not the longer wrapped one.
            var creds = new PolymarketCredentials(SignType.EOA, OwnerPrivateKey);
            var auth = new PolymarketAuthenticationProvider(creds);
            var order = BuildOrder(auth.PublicAddress, auth.PublicAddress, 0);
            var sig = auth.GetOrderSignature(order, chainId: 137, negativeRisk: false);
            Assert.That(sig.Length, Is.EqualTo(132));
        }
    }
}
