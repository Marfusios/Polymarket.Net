using System;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Token info
    /// </summary>
    public record PolymarketOrderBook
    {
        /// <summary>
        /// Market
        /// </summary>
        [JsonPropertyName("market")]
        public string Market { get; set; } = string.Empty;
        /// <summary>
        /// Asset id
        /// </summary>
        [JsonPropertyName("asset_id")]
        public string AssetId { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Hash
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// Bids
        /// </summary>
        [JsonPropertyName("bids")]
        public PolymarketBookEntry[] Bids { get; set; } = [];
        /// <summary>
        /// Asks
        /// </summary>
        [JsonPropertyName("asks")]
        public PolymarketBookEntry[] Asks { get; set; } = [];
        /// <summary>
        /// Min order quantity
        /// </summary>
        [JsonPropertyName("min_order_size")]
        public decimal MinOrderQuantity { get; set; }
        /// <summary>
        /// Tick quantity
        /// </summary>
        [JsonPropertyName("tick_size")]
        public decimal TickQuantity { get; set; }
        /// <summary>
        /// Negative risk enabled
        /// </summary>
        [JsonPropertyName("neg_risk")]
        public bool NegativeRisk { get; set; }
    }

    /// <summary>
    /// Order book entry
    /// </summary>
    public record PolymarketBookEntry
    {
        /// <summary>
        /// Price
        /// </summary>
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        [JsonPropertyName("size")]
        public decimal Quantity { get; set; }
    }
}
