using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Sport info
    /// </summary>
    public record PolymarketSport
    {
        /// <summary>
        /// Sport
        /// </summary>
        [JsonPropertyName("sport")]
        public string Sport { get; set; } = string.Empty;
        /// <summary>
        /// Image
        /// </summary>
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;
        /// <summary>
        /// Resolution
        /// </summary>
        [JsonPropertyName("resolution")]
        public string Resolution { get; set; } = string.Empty;
        /// <summary>
        /// Ordering
        /// </summary>
        [JsonPropertyName("ordering")]
        public string Ordering { get; set; } = string.Empty;
        /// <summary>
        /// Tags
        /// </summary>
        [JsonPropertyName("tags"), JsonConverter(typeof(CommaSplitStringConverter))]
        public string[] Tags { get; set; } = [];
        /// <summary>
        /// Series
        /// </summary>
        [JsonPropertyName("series")]
        public string Series { get; set; } = string.Empty;
    }
}
