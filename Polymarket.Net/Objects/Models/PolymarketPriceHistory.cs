using System;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Objects.Models
{
    internal record PolymarketPriceHistoryWrapper
    {
        /// <summary>
        /// History
        /// </summary>
        [JsonPropertyName("history")]
        public PolymarketPriceHistory[] History { get; set; } = [];
    }

    /// <summary>
    /// Price history
    /// </summary>
    public record PolymarketPriceHistory
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        [JsonPropertyName("t")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        [JsonPropertyName("p")]
        public decimal Price { get; set; }
    }


}
