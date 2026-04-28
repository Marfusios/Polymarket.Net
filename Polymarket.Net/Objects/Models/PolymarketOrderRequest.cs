using Polymarket.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Order request
    /// </summary>
    public record PolymarketOrderRequest
    {
        /// <summary>
        /// Token id
        /// </summary>
        public string TokenId { get; set; } = string.Empty;
        /// <summary>
        /// Order side
        /// </summary>
        public OrderSide Side { get; set; }
        /// <summary>
        /// Order type
        /// </summary>
        public OrderType OrderType { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        public decimal? Price { get; set; }
        /// <summary>
        /// Time in force
        /// </summary>
        public TimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Is post only
        /// </summary>
        public bool? PostOnly { get; set; }
        /// <summary>
        /// Metadata bytes32
        /// </summary>
        public string? Metadata { get; set; }
        /// <summary>
        /// Builder attribution code bytes32
        /// </summary>
        public string? BuilderCode { get; set; }
        /// <summary>
        /// Client order id
        /// </summary>
        public long? ClientOrderId { get; set; }
        /// <summary>
        /// Expiration
        /// </summary>
        public DateTime? Expiration { get; set; }
        /// <summary>
        /// Defer execution
        /// </summary>
        public bool? DeferExecution { get; set; }

    }
}
