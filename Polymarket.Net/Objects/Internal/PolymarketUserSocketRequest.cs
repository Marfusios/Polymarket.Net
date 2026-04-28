using System.Text.Json.Serialization;

namespace Polymarket.Net.Objects.Internal
{
    internal class PolymarketUserSocketRequest
    {
        [JsonPropertyName("markets")]
        public string[] Markets { get; set; } = [];

        [JsonPropertyName("operation")]
        public string Operation { get; set; } = string.Empty;
    }
}
