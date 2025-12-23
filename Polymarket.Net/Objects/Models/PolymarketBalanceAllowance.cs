using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Balance allowance
    /// </summary>
    public record PolymarketBalanceAllowance
    {
        /// <summary>
        /// Balance
        /// </summary>
        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }
        /// <summary>
        /// Allowances
        /// </summary>
        [JsonPropertyName("allowances")]
        public Dictionary<string, decimal> Allowances { get; set; } = new();
    }
}
