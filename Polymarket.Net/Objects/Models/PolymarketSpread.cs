using System.Text.Json.Serialization;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Spread info
    /// </summary>
    public record PolymarketSpread
    {
        /// <summary>
        /// Spread
        /// </summary>
        [JsonPropertyName("spread")]
        public decimal Spread { get; set; }
    }
}
