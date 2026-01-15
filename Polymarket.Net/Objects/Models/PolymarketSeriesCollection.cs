using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Series collection
    /// </summary>
    public record PolymarketSeriesCollection
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Ticker
        /// </summary>
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; } = string.Empty;
        /// <summary>
        /// Slug
        /// </summary>
        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;
        /// <summary>
        /// Title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Subtitle
        /// </summary>
        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; } = string.Empty;
        /// <summary>
        /// Collection type
        /// </summary>
        [JsonPropertyName("collectionType")]
        public string CollectionType { get; set; } = string.Empty;
        /// <summary>
        /// Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Tags
        /// </summary>
        [JsonPropertyName("tags")]
        public string Tags { get; set; } = string.Empty;
        /// <summary>
        /// Image
        /// </summary>
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;
        /// <summary>
        /// Icon
        /// </summary>
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
        /// <summary>
        /// Header image
        /// </summary>
        [JsonPropertyName("headerImage")]
        public string HeaderImage { get; set; } = string.Empty;
        /// <summary>
        /// Layout
        /// </summary>
        [JsonPropertyName("layout")]
        public string Layout { get; set; } = string.Empty;
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
        /// New
        /// </summary>
        [JsonPropertyName("new")]
        public bool New { get; set; }
        /// <summary>
        /// Featured
        /// </summary>
        [JsonPropertyName("featured")]
        public bool Featured { get; set; }
        /// <summary>
        /// Restricted
        /// </summary>
        [JsonPropertyName("restricted")]
        public bool Restricted { get; set; }
        /// <summary>
        /// Is template
        /// </summary>
        [JsonPropertyName("isTemplate")]
        public bool IsTemplate { get; set; }
        /// <summary>
        /// Template variables
        /// </summary>
        [JsonPropertyName("templateVariables")]
        public string TemplateVariables { get; set; } = string.Empty;
        /// <summary>
        /// Publish time
        /// </summary>
        [JsonPropertyName("publishedAt")]
        public DateTime? PublishTime { get; set; }
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
        /// <summary>
        /// Comments enabled
        /// </summary>
        [JsonPropertyName("commentsEnabled")]
        public bool CommentsEnabled { get; set; }
        /// <summary>
        /// Image optimized
        /// </summary>
        [JsonPropertyName("imageOptimized")]
        public PolymarketImageRef? ImageOptimized { get; set; } = null!;
        /// <summary>
        /// Icon optimized
        /// </summary>
        [JsonPropertyName("iconOptimized")]
        public PolymarketImageRef? IconOptimized { get; set; } = null!;
        /// <summary>
        /// Header image optimized
        /// </summary>
        [JsonPropertyName("headerImageOptimized")]
        public PolymarketImageRef? HeaderImageOptimized { get; set; } = null!;
    }
}
