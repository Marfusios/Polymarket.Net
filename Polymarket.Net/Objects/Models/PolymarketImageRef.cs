using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Image reference
    /// </summary>
    public record PolymarketImageRef
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Image url source
        /// </summary>
        [JsonPropertyName("imageUrlSource")]
        public string ImageUrlSource { get; set; } = string.Empty;
        /// <summary>
        /// Image url optimized
        /// </summary>
        [JsonPropertyName("imageUrlOptimized")]
        public string ImageUrlOptimized { get; set; } = string.Empty;
        /// <summary>
        /// Image quantity kb source
        /// </summary>
        [JsonPropertyName("imageSizeKbSource")]
        public long ImageQuantityKbSource { get; set; }
        /// <summary>
        /// Image quantity kb optimized
        /// </summary>
        [JsonPropertyName("imageSizeKbOptimized")]
        public long ImageQuantityKbOptimized { get; set; }
        /// <summary>
        /// Image optimized complete
        /// </summary>
        [JsonPropertyName("imageOptimizedComplete")]
        public bool ImageOptimizedComplete { get; set; }
        /// <summary>
        /// Image optimized last updated
        /// </summary>
        [JsonPropertyName("imageOptimizedLastUpdated")]
        public DateTime ImageOptimizedLastUpdated { get; set; }
        /// <summary>
        /// Related id
        /// </summary>
        [JsonPropertyName("relID")]
        public decimal RelId { get; set; }
        /// <summary>
        /// Field
        /// </summary>
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
        /// <summary>
        /// Relname
        /// </summary>
        [JsonPropertyName("relname")]
        public string Relname { get; set; } = string.Empty;
    }


}
