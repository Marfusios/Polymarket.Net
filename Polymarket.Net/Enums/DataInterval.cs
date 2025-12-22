using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Enums
{
    /// <summary>
    /// Data interval
    /// </summary>
    [JsonConverter(typeof(EnumConverter<DataInterval>))]
    public enum DataInterval
    {
        /// <summary>
        /// One hour
        /// </summary>
        [Map("1h")]
        OneHour,
        /// <summary>
        /// Six hours
        /// </summary>
        [Map("6h")]
        SixHours,
        /// <summary>
        /// One day
        /// </summary>
        [Map("1d")]
        OneDay,
        /// <summary>
        /// One week
        /// </summary>
        [Map("1w")]
        OneWeek,
        /// <summary>
        /// One month
        /// </summary>
        [Map("1m")]
        OneMonth,
        /// <summary>
        /// Max
        /// </summary>
        [Map("max")]
        Max
    }
}
