using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Template
    /// </summary>
    public record PolymarketTemplate
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Event title
        /// </summary>
        [JsonPropertyName("eventTitle")]
        public string EventTitle { get; set; } = string.Empty;
        /// <summary>
        /// Event slug
        /// </summary>
        [JsonPropertyName("eventSlug")]
        public string EventSlug { get; set; } = string.Empty;
        /// <summary>
        /// Event image
        /// </summary>
        [JsonPropertyName("eventImage")]
        public string EventImage { get; set; } = string.Empty;
        /// <summary>
        /// Market title
        /// </summary>
        [JsonPropertyName("marketTitle")]
        public string MarketTitle { get; set; } = string.Empty;
        /// <summary>
        /// Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Resolution source
        /// </summary>
        [JsonPropertyName("resolutionSource")]
        public string ResolutionSource { get; set; } = string.Empty;
        /// <summary>
        /// Negative risk
        /// </summary>
        [JsonPropertyName("negRisk")]
        public bool NegativeRisk { get; set; }
        /// <summary>
        /// Sort by
        /// </summary>
        [JsonPropertyName("sortBy")]
        public string SortBy { get; set; } = string.Empty;
        /// <summary>
        /// Show market images
        /// </summary>
        [JsonPropertyName("showMarketImages")]
        public bool ShowMarketImages { get; set; }
        /// <summary>
        /// Series slug
        /// </summary>
        [JsonPropertyName("seriesSlug")]
        public string SeriesSlug { get; set; } = string.Empty;
        /// <summary>
        /// Outcomes
        /// </summary>
        [JsonPropertyName("outcomes")]
        public string Outcomes { get; set; } = string.Empty;
    }
}
