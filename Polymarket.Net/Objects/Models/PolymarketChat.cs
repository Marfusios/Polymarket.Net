using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Chat info
    /// </summary>
    public record PolymarketChat
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Channel id
        /// </summary>
        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; } = string.Empty;
        /// <summary>
        /// Channel name
        /// </summary>
        [JsonPropertyName("channelName")]
        public string ChannelName { get; set; } = string.Empty;
        /// <summary>
        /// Channel image
        /// </summary>
        [JsonPropertyName("channelImage")]
        public string ChannelImage { get; set; } = string.Empty;
        /// <summary>
        /// Live
        /// </summary>
        [JsonPropertyName("live")]
        public bool Live { get; set; }
        /// <summary>
        /// Start time
        /// </summary>
        [JsonPropertyName("startTime")]
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// End time
        /// </summary>
        [JsonPropertyName("endTime")]
        public DateTime? EndTime { get; set; }
    }

}
