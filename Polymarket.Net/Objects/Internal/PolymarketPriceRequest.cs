using Polymarket.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects.Internal
{
    internal record PolymarketPriceRequest
    {
        [JsonPropertyName("token_id")]
        public string TokenId { get; set; } = string.Empty;
        [JsonPropertyName("side")]
        public OrderSide Side { get; set; }
    }
}
