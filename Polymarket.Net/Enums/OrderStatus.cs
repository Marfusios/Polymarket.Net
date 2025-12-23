using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Enums
{
    /// <summary>
    /// Order status
    /// </summary>
    [JsonConverter(typeof(EnumConverter<OrderStatus>))]
    public enum OrderStatus
    {
        /// <summary>
        /// Live
        /// </summary>
        [Map("LIVE")]
        Live,
        /// <summary>
        /// Canceled
        /// </summary>
        [Map("CANCELED")]
        Canceled,
    }
}
