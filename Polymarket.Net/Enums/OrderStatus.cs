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
        [Map("LIVE", "live")]
        Live,
        /// <summary>
        /// Canceled
        /// </summary>
        [Map("CANCELED", "canceled")]
        Canceled,
        /// <summary>
        /// Matched
        /// </summary>
        [Map("MATCHED", "matched")]
        Matched,
        /// <summary>
        /// Delayed
        /// </summary>
        [Map("DELAYED", "delayed")]
        Delayed
    }
}
