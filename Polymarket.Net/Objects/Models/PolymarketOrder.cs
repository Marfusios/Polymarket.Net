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
    /// Order info
    /// </summary>
    public record PolymarketOrder: PolymarketOrderBase
    {
        /// <summary>
        /// Any trade id the order has been partially included in
        /// </summary>
        [JsonPropertyName("associate_trades")]
        public string[] TradeIds { get; set; } = [];

        /// <summary>
        /// Original order quantity at placement
        /// </summary>
        [JsonPropertyName("original_size")]
        public decimal OriginalQuantity { get; set; }
        /// <summary>
        /// Expiration time
        /// </summary>
        [JsonPropertyName("expiration")]
        public DateTime? Expiration { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        [JsonPropertyName("status")]
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Time in force
        /// </summary>
        [JsonPropertyName("type")]
        public TimeInForce TimeInForce { get; set; }

        [JsonInclude, JsonPropertyName("order_type")]
        internal TimeInForce TimeInForceInt
        {
            get => TimeInForce;
            set => TimeInForce = value;
        }
        /// <summary>
        /// Create time
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreateTime { get; set; }
    }
}
