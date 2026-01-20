using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Data page
    /// </summary>
    public record PolymarketPage<T>
    {
        /// <summary>
        /// Pagination cursor
        /// </summary>
        [JsonPropertyName("next_cursor")]
        public string NextPageCursor { get; set; } = string.Empty;
        /// <summary>
        /// Max number of results
        /// </summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        /// <summary>
        /// Number of results
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }
        /// <summary>
        /// Page data
        /// </summary>
        [JsonPropertyName("data")]
        public T[] Data { get; set; } = [];
    }
}
