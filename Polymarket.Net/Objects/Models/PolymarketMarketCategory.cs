using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Category info
    /// </summary>
    public record PolymarketMarketCategory
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
        /// Parent category
        /// </summary>
        [JsonPropertyName("parentCategory")]
        public string ParentCategory { get; set; } = string.Empty;
        /// <summary>
        /// Slug
        /// </summary>
        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;
        /// <summary>
        /// Publish time
        /// </summary>
        [JsonPropertyName("publishedAt")]
        public DateTime PublishTime { get; set; }
        /// <summary>
        /// Created by
        /// </summary>
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;
        /// <summary>
        /// Updated by
        /// </summary>
        [JsonPropertyName("updatedBy")]
        public string UpdatedBy { get; set; } = string.Empty;
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
