using System;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Heartbeat response
    /// </summary>
    public record PolymarketHeartbeatResult
    {
        /// <summary>
        /// Optional status string returned by server
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Optional success flag returned by server
        /// </summary>
        [JsonPropertyName("success")]
        public bool? Success { get; set; }

        /// <summary>
        /// Optional error message returned by server
        /// </summary>
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        /// <summary>
        /// Whether the response indicates success
        /// </summary>
        [JsonIgnore]
        public bool IsOk =>
            Success ?? string.Equals(Status, "ok", StringComparison.OrdinalIgnoreCase);
    }
}
