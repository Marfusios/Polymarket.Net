using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Enums
{
    /// <summary>
    /// Asset type
    /// </summary>
    [JsonConverter(typeof(EnumConverter<AssetType>))]
    public enum AssetType
    {
        /// <summary>
        /// Collateral
        /// </summary>
        [Map("COLLATERAL")]
        Collateral,
        /// <summary>
        /// Conditional
        /// </summary>
        [Map("CONDITIONAL")]
        Conditional
    }
}
