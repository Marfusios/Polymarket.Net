using Polymarket.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Last trade price
    /// </summary>
    public record PolymarketTradePrice
    {
        /// <summary>
        /// Last trade price
        /// </summary>
        [JsonPropertyName("price")]
        public decimal LastTradePrice { get; set; }

        /// <summary>
        /// Order side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide Side { get; set; }
        /// <summary>
        /// Token id
        /// </summary>
        [JsonPropertyName("token_id")]
        public string? TokenId { get; set; }
    }
}
