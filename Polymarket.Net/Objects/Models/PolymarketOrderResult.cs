using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Order result
    /// </summary>
    public record PolymarketOrderResult
    {
        /// <summary>
        /// Whether order was successful
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        /// <summary>
        /// Error message if failed
        /// </summary>
        [JsonPropertyName("errorMsg")]
        public string? Error { get; set; }
        /// <summary>
        /// Order id
        /// </summary>
        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }
        /// <summary>
        /// Hashes of trades which were executed immediately
        /// </summary>
        [JsonPropertyName("orderHashes")]
        public string[] TradeHashes { get; set; } = [];
    }
}
