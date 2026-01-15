using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Related tags
    /// </summary>
    public record PolymarketRelatedTag
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Tag id
        /// </summary>
        [JsonPropertyName("tagID")]
        public long TagId { get; set; }
        /// <summary>
        /// Related tag id
        /// </summary>
        [JsonPropertyName("relatedTagID")]
        public long RelatedTagId { get; set; }
        /// <summary>
        /// Rank
        /// </summary>
        [JsonPropertyName("rank")]
        public long Rank { get; set; }
    }
}
