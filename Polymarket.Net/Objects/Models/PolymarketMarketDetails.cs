using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// 
    /// </summary>
    public record PolymarketMarketDetails
    {
        /// <summary>
        /// Enable order book
        /// </summary>
        [JsonPropertyName("enable_order_book")]
        public bool EnableOrderBook { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        /// <summary>
        /// Closed
        /// </summary>
        [JsonPropertyName("closed")]
        public bool Closed { get; set; }
        /// <summary>
        /// Archived
        /// </summary>
        [JsonPropertyName("archived")]
        public bool Archived { get; set; }
        /// <summary>
        /// Accepting orders
        /// </summary>
        [JsonPropertyName("accepting_orders")]
        public bool AcceptingOrders { get; set; }
        /// <summary>
        /// Accepting order timestamp
        /// </summary>
        [JsonPropertyName("accepting_order_timestamp")]
        public DateTime? AcceptingOrderTimestamp { get; set; }
        /// <summary>
        /// Minimum order quantity
        /// </summary>
        [JsonPropertyName("minimum_order_size")]
        public decimal MinimumOrderQuantity { get; set; }
        /// <summary>
        /// Minimum tick quantity
        /// </summary>
        [JsonPropertyName("minimum_tick_size")]
        public decimal MinimumTickQuantity { get; set; }
        /// <summary>
        /// Condition id
        /// </summary>
        [JsonPropertyName("condition_id")]
        public string ConditionId { get; set; } = string.Empty;
        /// <summary>
        /// Question id
        /// </summary>
        [JsonPropertyName("question_id")]
        public string QuestionId { get; set; } = string.Empty;
        /// <summary>
        /// Question
        /// </summary>
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;
        /// <summary>
        /// Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Market slug
        /// </summary>
        [JsonPropertyName("market_slug")]
        public string MarketSlug { get; set; } = string.Empty;
        /// <summary>
        /// End time
        /// </summary>
        [JsonPropertyName("end_date_iso")]
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// Game start time
        /// </summary>
        [JsonPropertyName("game_start_time")]
        public DateTime? GameStartTime { get; set; }
        /// <summary>
        /// Seconds delay
        /// </summary>
        [JsonPropertyName("seconds_delay")]
        public long SecondsDelay { get; set; }
        /// <summary>
        /// Fpmm
        /// </summary>
        [JsonPropertyName("fpmm")]
        public string? Fpmm { get; set; }
        /// <summary>
        /// Maker base fee
        /// </summary>
        [JsonPropertyName("maker_base_fee")]
        public decimal MakerBaseFee { get; set; }
        /// <summary>
        /// Taker base fee
        /// </summary>
        [JsonPropertyName("taker_base_fee")]
        public decimal TakerBaseFee { get; set; }
        /// <summary>
        /// Notifications enabled
        /// </summary>
        [JsonPropertyName("notifications_enabled")]
        public bool NotificationsEnabled { get; set; }
        /// <summary>
        /// Negative risk enabled
        /// </summary>
        [JsonPropertyName("neg_risk")]
        public bool NegativeRisk { get; set; }
        /// <summary>
        /// Negative risk market id
        /// </summary>
        [JsonPropertyName("neg_risk_market_id")]
        public string NegativeRiskMarketId { get; set; } = string.Empty;
        /// <summary>
        /// Negative risk request id
        /// </summary>
        [JsonPropertyName("neg_risk_request_id")]
        public string NegativeRiskRequestId { get; set; } = string.Empty;
        /// <summary>
        /// Icon
        /// </summary>
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
        /// <summary>
        /// Image
        /// </summary>
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;
        /// <summary>
        /// Rewards
        /// </summary>
        [JsonPropertyName("rewards")]
        public PolymarketMarketReward Rewards { get; set; } = null!;
        /// <summary>
        /// Is 50/50 outcome
        /// </summary>
        [JsonPropertyName("is_50_50_outcome")]
        public bool Is5050Outcome { get; set; }
        /// <summary>
        /// Tokens
        /// </summary>
        [JsonPropertyName("tokens")]
        public PolymarketMarketToken[] Tokens { get; set; } = [];
        /// <summary>
        /// Tags
        /// </summary>
        [JsonPropertyName("tags")]
        public string[] Tags { get; set; } = [];
    }
}
