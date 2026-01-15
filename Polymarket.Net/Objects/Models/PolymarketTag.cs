using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Tag info
    /// </summary>
    public record PolymarketTag
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Label
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;
        /// <summary>
        /// Slug
        /// </summary>
        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;
        /// <summary>
        /// Force show
        /// </summary>
        [JsonPropertyName("forceShow")]
        public bool ForceShow { get; set; }
        /// <summary>
        /// Force hide
        /// </summary>
        [JsonPropertyName("forceHide")]
        public bool ForceHide { get; set; }
        /// <summary>
        /// Is carousel
        /// </summary>
        [JsonPropertyName("isCarousel")]
        public bool IsCarousel { get; set; }
        /// <summary>
        /// Publish time
        /// </summary>
        [JsonPropertyName("publishedAt")]
        public DateTime PublishTime { get; set; }
        /// <summary>
        /// Create time
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Update time
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime UpdateTime { get; set; }
    }
}
