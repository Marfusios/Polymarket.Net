using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Tick size
    /// </summary>
    public record PolymarketTickSize
    {
        /// <summary>
        /// Minimum tick size
        /// </summary>
        [JsonPropertyName("minimum_tick_size")]
        public decimal MinTickSize { get; set; }
    }
}
