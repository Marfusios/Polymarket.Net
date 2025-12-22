using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Enums
{
    /// <summary>
    /// Time in force
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TimeInForce>))]
    public enum TimeInForce
    {
        /// <summary>
        /// Fill immediately or cancel
        /// </summary>
        [Map("FOK")]
        FillOrKill,
        /// <summary>
        /// Fill immediately or cancel
        /// </summary>
        [Map("FAK")]
        ImmediateOrCancel,
        /// <summary>
        /// Good until canceled
        /// </summary>
        [Map("GTC")]
        GoodTillCanceled,
        /// <summary>
        /// Good till specific timestamp
        /// </summary>
        [Map("GTD")]
        GoodTillDate
    }
}
