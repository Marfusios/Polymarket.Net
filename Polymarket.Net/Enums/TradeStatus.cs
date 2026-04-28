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
        [Map("MATCHED", "TRADE_STATUS_MATCHED")]
        Matched,
        /// <summary>
        /// Mined
        /// </summary>
        [Map("MINED", "TRADE_STATUS_MINED")]
        Mined,
        /// <summary>
        /// Confirmed
        /// </summary>
        [Map("CONFIRMED", "TRADE_STATUS_CONFIRMED")]
        Confirmed,
        /// <summary>
        /// Retrying
        /// </summary>
        [Map("RETRYING", "TRADE_STATUS_RETRYING")]
        Retrying,
        /// <summary>
        /// Failed
        /// </summary>
        [Map("FAILED", "TRADE_STATUS_FAILED")]
        Failed,
    }
}
