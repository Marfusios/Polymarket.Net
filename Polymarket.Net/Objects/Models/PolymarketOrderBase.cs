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
    /// Order info
    /// </summary>
    public record PolymarketOrderBase
    {
        /// <summary>
        /// Order id
        /// </summary>
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("id")]
        internal string OrderIdInt
        {
            get => OrderId;
            set => OrderId = value;
        }

        /// <summary>
        /// Maker address
        /// </summary>
        [JsonPropertyName("maker_address")]
        public string MakerAddress { get; set; } = string.Empty;
        /// <summary>
        /// Asset/token id
        /// </summary>
        [JsonPropertyName("asset_id")]
        public string TokenId { get; set; } = string.Empty;
        /// <summary>
        /// API key of the order owner
        /// </summary>
        [JsonPropertyName("owner")]
        public string Owner { get; set; } = string.Empty;
        /// <summary>
        /// Quantity matched in trade
        /// </summary>
        [JsonPropertyName("matched_amount")]
        public decimal QuantityFilled { get; set; }

        [JsonInclude, JsonPropertyName("size_matched")]
        internal decimal QuantityFilledInt
        {
            get => QuantityFilled;
            set => QuantityFilled = value;
        }

        /// <summary>
        /// Fee rate in BPS
        /// </summary>
        [JsonPropertyName("fee_rate_bps")]
        public decimal FeeRateBps { get; set; }
        /// <summary>
        /// Price
        /// </summary>
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        /// <summary>
        /// Order outcome description
        /// </summary>
        [JsonPropertyName("outcome")]
        public string Outcome { get; set; } = string.Empty;
        /// <summary>
        /// Order side
        /// </summary>
        [JsonPropertyName("side")]
        public OrderSide Side { get; set; }
    }
}
