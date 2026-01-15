using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Creator info
    /// </summary>
    public record PolymarketCreator
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Creator name
        /// </summary>
        [JsonPropertyName("creatorName")]
        public string CreatorName { get; set; } = string.Empty;
        /// <summary>
        /// Creator handle
        /// </summary>
        [JsonPropertyName("creatorHandle")]
        public string CreatorHandle { get; set; } = string.Empty;
        /// <summary>
        /// Creator URL
        /// </summary>
        [JsonPropertyName("creatorUrl")]
        public string CreatorUrl { get; set; } = string.Empty;
        /// <summary>
        /// Creator image
        /// </summary>
        [JsonPropertyName("creatorImage")]
        public string CreatorImage { get; set; } = string.Empty;
        /// <summary>
        /// Create time
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Update time
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime? UpdateTime { get; set; }
    }
}
