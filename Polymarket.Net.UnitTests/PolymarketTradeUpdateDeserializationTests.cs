using NUnit.Framework;
using Polymarket.Net.Objects.Models;
using System.Text.Json;

namespace Polymarket.Net.UnitTests
{
    [TestFixture]
    public class PolymarketTradeUpdateDeserializationTests
    {
        // Captured live from /ws/user against traxtop credentials on 2026-04-28. Real fill
        // (10 shares @ 0.6 against the bot's own GTC order) where every maker_orders[*]
        // entry has fee_rate_bps:"" — empty string. Without the converter on FeeRateBps,
        // System.Text.Json throws "could not be converted to System.Decimal" and the
        // entire frame is dropped, which is how Polyharvest stopped seeing fills after
        // the v2 upgrade.
        private const string MakerEmptyFeeRateBpsTradePayload =
            "{\"type\":\"TRADE\", \"id\":\"bc7a8968-5e09-48d8-93e0-b4a32bb63f99\", " +
            "\"taker_order_id\":\"0xab609a0d99e4881e6e225287e33f806db7d3cfbab7a51e68695f24d986b332ce\", " +
            "\"market\":\"0x9931ef80708e0527491815677b6914a9aa33aeda9da6d732b049915671edb7da\", " +
            "\"asset_id\":\"30706638781166263128652253423517818078462715745939226032878677024292835136000\", " +
            "\"side\":\"BUY\", \"size\":\"111.229823\", \"fee_rate_bps\":\"0\", \"price\":\"0.43\", " +
            "\"status\":\"MATCHED\", \"match_time\":\"1777411180\", \"last_update\":\"1777411180\", " +
            "\"outcome\":\"Down\", \"owner\":\"30692a6f-2445-d1f9-6968-ee7ba4f71ee0\", " +
            "\"trade_owner\":\"30692a6f-2445-d1f9-6968-ee7ba4f71ee0\", " +
            "\"maker_address\":\"0xE4B03777FAE459db918FFCcFf9ACE094AA64bb1D\", " +
            "\"transaction_hash\":\"0x150d62c78e391f2cef716d8138ccf34748c535654fa45a0ee5adf6a97f9b00be\", " +
            "\"bucket_index\":0, \"maker_orders\":[" +
            "{\"order_id\":\"0xaf98485f60dd05bc41b49ce8071815347ed513eefeb4aca7b7be51c5e6b0456d\", " +
            "\"owner\":\"d5eb12b4-506a-6c6f-5e76-666066189488\", " +
            "\"maker_address\":\"0xb07AfA532E202d951ED6f1C00c2519b6f6061328\", " +
            "\"matched_amount\":\"10\", \"price\":\"0.6\", \"fee_rate_bps\":\"\", " +
            "\"asset_id\":\"40885369797799221015516051756208345780335339509906249684034926889003302254367\", " +
            "\"outcome\":\"Up\", \"outcome_index\":0, \"side\":\"BUY\"}], " +
            "\"trader_side\":\"MAKER\", \"timestamp\":\"1777411180352\", \"event_type\":\"trade\"}";

        [Test]
        public void TradeUpdate_WithEmptyFeeRateBpsInMakerOrders_DeserializesWithoutThrowing()
        {
            var trade = JsonSerializer.Deserialize<PolymarketTradeUpdate>(
                MakerEmptyFeeRateBpsTradePayload,
                PolymarketPlatform._serializerContext);

            Assert.That(trade, Is.Not.Null);
            Assert.That(trade!.MakerOrders.Length, Is.EqualTo(1));
            Assert.That(trade.MakerOrders[0].FeeRateBps, Is.EqualTo(0m), "Empty fee_rate_bps must coerce to 0");
            Assert.That(trade.MakerOrders[0].QuantityFilled, Is.EqualTo(10m));
            Assert.That(trade.MakerOrders[0].Price, Is.EqualTo(0.6m));
            Assert.That(trade.TradeId, Is.EqualTo("bc7a8968-5e09-48d8-93e0-b4a32bb63f99"));
            Assert.That(trade.Quantity, Is.EqualTo(111.229823m));
        }
    }
}
