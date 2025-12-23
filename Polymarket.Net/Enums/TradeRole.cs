using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Enums
{
    /// <summary>
    /// Trade role
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TradeRole>))]
    public enum TradeRole
    {
        /// <summary>
        /// Taker
        /// </summary>
        [Map("TAKER")]
        Taker,
        /// <summary>
        /// Maker
        /// </summary>
        [Map("MAKER")]
        Maker
    }
}
