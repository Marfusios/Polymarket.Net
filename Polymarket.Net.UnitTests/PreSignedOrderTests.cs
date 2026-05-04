using NUnit.Framework;
using Polymarket.Net.Clients.ClobApi;
using Polymarket.Net.Enums;

namespace Polymarket.Net.UnitTests
{
    /// <summary>
    /// Tests for the pre-sign-aware refactor of <see cref="PolymarketRestClientClobApiTrading"/>.
    /// We exercise the static helpers that the new <c>BuildAndSignOrder</c> path depends on —
    /// the dynamic build+sign requires a fully wired client which is covered by the
    /// integration test fixture, not unit tests.
    /// </summary>
    [TestFixture]
    public class PreSignedOrderTests
    {
        // ── ComputeLimitQuantities ──

        [Test]
        public void ComputeLimitQuantities_Buy_UsesPriceTimesQuantity_AsMaker()
        {
            // 10 shares at $0.50 → maker (USDC) = 10 * 0.50 = 5.0; taker (shares) = 10
            var (maker, taker) = PolymarketRestClientClobApiTrading
                .ComputeLimitQuantities(OrderSide.Buy, quantity: 10m, normalizedPrice: 0.50m);
            // CLOB base units = amount * 1_000_000 (truncated)
            Assert.That(maker, Is.EqualTo(5_000_000m));
            Assert.That(taker, Is.EqualTo(10_000_000m));
        }

        [Test]
        public void ComputeLimitQuantities_Sell_UsesQuantityTimesPrice_AsTaker()
        {
            // Sell side flips: maker = shares, taker = USDC.
            var (maker, taker) = PolymarketRestClientClobApiTrading
                .ComputeLimitQuantities(OrderSide.Sell, quantity: 10m, normalizedPrice: 0.50m);
            Assert.That(maker, Is.EqualTo(10_000_000m));
            Assert.That(taker, Is.EqualTo(5_000_000m));
        }

        [Test]
        public void ComputeLimitQuantities_PriceRoundedDownInTakerComputation()
        {
            // 7 shares @ $0.123: maker = 7 * 0.123 = 0.861. takerQty pre-base-units = 7.
            // After rounding/base-unit conversion, both are integer micro-USDC/shares.
            var (maker, taker) = PolymarketRestClientClobApiTrading
                .ComputeLimitQuantities(OrderSide.Buy, quantity: 7m, normalizedPrice: 0.123m);
            Assert.That(maker, Is.EqualTo(861_000m));   // 0.861 USDC
            Assert.That(taker, Is.EqualTo(7_000_000m)); // 7 shares
        }

        // ── NormalizeOrderPrice ──

        [Test]
        public void NormalizeOrderPrice_RoundsToThreeDecimals()
        {
            Assert.That(PolymarketRestClientClobApiTrading.NormalizeOrderPrice(0.4567m), Is.EqualTo(0.457m));
            Assert.That(PolymarketRestClientClobApiTrading.NormalizeOrderPrice(0.5m), Is.EqualTo(0.5m));
            Assert.That(PolymarketRestClientClobApiTrading.NormalizeOrderPrice(0.5005m), Is.EqualTo(0.5m).Within(0.0001m));
        }

        // ── ConvertToClobBaseUnits ──

        [Test]
        public void ConvertToClobBaseUnits_MultipliesByMillion_AndTruncates()
        {
            Assert.That(PolymarketRestClientClobApiTrading.ConvertToClobBaseUnits(1m), Is.EqualTo(1_000_000m));
            Assert.That(PolymarketRestClientClobApiTrading.ConvertToClobBaseUnits(0.5m), Is.EqualTo(500_000m));
            Assert.That(PolymarketRestClientClobApiTrading.ConvertToClobBaseUnits(0.123456m), Is.EqualTo(123_456m));
            // Truncation, not rounding.
            Assert.That(PolymarketRestClientClobApiTrading.ConvertToClobBaseUnits(0.1234569m), Is.EqualTo(123_456m));
        }

        // ── NormalizeBytes32 ──

        [Test]
        public void NormalizeBytes32_NullOrWhitespace_ReturnsZeroBytes32()
        {
            const string zero = "0x0000000000000000000000000000000000000000000000000000000000000000";
            Assert.That(PolymarketRestClientClobApiTrading.NormalizeBytes32(null, "metadata"), Is.EqualTo(zero));
            Assert.That(PolymarketRestClientClobApiTrading.NormalizeBytes32("   ", "metadata"), Is.EqualTo(zero));
        }

        [Test]
        public void NormalizeBytes32_LowercasesValue()
        {
            const string mixed = "0xABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789";
            const string lower = "0xabcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789";
            Assert.That(PolymarketRestClientClobApiTrading.NormalizeBytes32(mixed, "builder"), Is.EqualTo(lower));
        }

        [Test]
        public void NormalizeBytes32_RejectsBadLength()
        {
            // 64 hex chars without 0x prefix should be rejected.
            const string bad = "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789";
            Assert.Throws<System.ArgumentException>(() =>
                PolymarketRestClientClobApiTrading.NormalizeBytes32(bad, "metadata"));
        }

        [Test]
        public void NormalizeBytes32_RejectsNonHexCharacters()
        {
            // 0x + 64 chars but contains non-hex
            const string bad = "0xZZZZ000000000000000000000000000000000000000000000000000000000000";
            Assert.Throws<System.ArgumentException>(() =>
                PolymarketRestClientClobApiTrading.NormalizeBytes32(bad, "metadata"));
        }
    }
}
