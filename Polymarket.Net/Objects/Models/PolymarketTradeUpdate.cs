using Polymarket.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// User trade update
    /// </summary>
    public record PolymarketTradeUpdate : PolymarketSocketUpdate
    {
        /// <summary>
        /// Trade id
        /// </summary>
        [JsonPropertyName("id")]
        public string TradeId { get; set; } = string.Empty;
        /// <summary>
        /// Taker order id
        /// </summary>
        [JsonPropertyName("taker_order_id")]
        public string TakerOrderId { get; set; } = string.Empty;
        /// <summary>
        /// Trade owner
        /// </summary>
        [JsonPropertyName("trade_owner")]
        public string TradeOwner { get; set; } = string.Empty;
        /// <summary>
        /// Condition/market id
        /// </summary>
        [JsonPropertyName("market")]
        public string ConditionId { get; set; } = string.Empty;
        /// <summary>
        /// Asset/token id
        /// </summary>
        [JsonPropertyName("asset_id")]
        public string TokenId { get; set; } = string.Empty;
        /// <summary>
        /// Maker address
        /// </summary>
        [JsonPropertyName("maker_address")]
        public string MakerAddress { get; set; } = string.Empty;
        /// <summary>
        /// Transaction hash
        /// </summary>
        [JsonPropertyName("transaction_hash")]
        public string TransactionHash { get; set; } = string.Empty;
        /// <summary>
        /// Side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide Side { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("size")]
        public decimal Quantity { get; set; }
        /// <summary>
        /// Fee rate BPS
        /// </summary>
        [JsonPropertyName("fee_rate_bps")]
        public decimal FeeRateBps { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        /// <summary>
        /// Trade status
        /// </summary>
        [JsonPropertyName("status")]
        public TradeStatus Status { get; set; }
        /// <summary>
        /// Matching time
        /// </summary>
        [JsonPropertyName("match_time")]
        public DateTime MatchTime { get; set; }
        /// <summary>
        /// Bucket index
        /// </summary>
        [JsonPropertyName("bucket_index")]
        public long BucketIndex { get; set; }
        /// <summary>
        /// Last update time
        /// </summary>
        [JsonPropertyName("last_update")]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// Outcome string
        /// </summary>
        [JsonPropertyName("outcome")]
        public string Outcome { get; set; } = string.Empty;
        /// <summary>
        /// API key of the taker of the trade
        /// </summary>
        [JsonPropertyName("owner")]
        public string Owner { get; set; } = string.Empty;
        /// <summary>
        /// Role
        /// </summary>
        [JsonPropertyName("trader_side")]
        public TradeRole TradeRole { get; set; }
        /// <summary>
        /// List of the maker trades the taker trade was filled against
        /// </summary>
        [JsonPropertyName("maker_orders")]
        public PolymarketOrderBase[] MakerOrders { get; set; } = [];
    }
}
