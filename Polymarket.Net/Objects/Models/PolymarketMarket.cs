using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Market info
    /// </summary>
    public record PolymarketMarket
    {
        /// <summary>
        /// Condition id
        /// </summary>
        [JsonPropertyName("condition_id")]
        public string ConditionId { get; set; } = string.Empty;
        /// <summary>
        /// Rewards
        /// </summary>
        [JsonPropertyName("rewards")]
        public PolymarketMarketReward Rewards { get; set; } = null!;
        /// <summary>
        /// Tokens
        /// </summary>
        [JsonPropertyName("tokens")]
        public PolymarketMarketToken[] Tokens { get; set; } = [];
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
    }

    /// <summary>
    /// 
    /// </summary>
    public record PolymarketMarketReward
    {
        /// <summary>
        /// Rates
        /// </summary>
        [JsonPropertyName("rates")]
        public PolymarketMarketRewardRates[] Rates { get; set; } = [];
        /// <summary>
        /// Min quantity
        /// </summary>
        [JsonPropertyName("min_size")]
        public decimal MinQuantity { get; set; }
        /// <summary>
        /// Max spread
        /// </summary>
        [JsonPropertyName("max_spread")]
        public decimal MaxSpread { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public record PolymarketMarketRewardRates
    {
        /// <summary>
        /// Asset address
        /// </summary>
        [JsonPropertyName("asset_address")]
        public string AssetAddress { get; set; } = string.Empty;
        /// <summary>
        /// Rewards daily rate
        /// </summary>
        [JsonPropertyName("rewards_daily_rate")]
        public decimal RewardsDailyRate { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public record PolymarketMarketToken
    {
        /// <summary>
        /// Token id
        /// </summary>
        [JsonPropertyName("token_id")]
        public string TokenId { get; set; } = string.Empty;
        /// <summary>
        /// Outcome
        /// </summary>
        [JsonPropertyName("outcome")]
        public string Outcome { get; set; } = string.Empty;
        /// <summary>
        /// Price
        /// </summary>
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        /// <summary>
        /// Winner
        /// </summary>
        [JsonPropertyName("winner")]
        public bool Winner { get; set; }
    }


}
