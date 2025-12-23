using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Enums
{
    /// <summary>
    /// Trade status
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TradeStatus>))]
    public enum TradeStatus
    {
        /// <summary>
        /// Matched
        /// </summary>
        [Map("MATCHED")]
        Matched,
        /// <summary>
        /// Mined
        /// </summary>
        [Map("MINED")]
        Mined,
        /// <summary>
        /// Confirmed
        /// </summary>
        [Map("CONFIRMED")]
        Confirmed,
        /// <summary>
        /// Retrying
        /// </summary>
        [Map("RETRYING")]
        Retrying,
        /// <summary>
        /// Failed
        /// </summary>
        [Map("FAILED")]
        Failed,
    }
}
