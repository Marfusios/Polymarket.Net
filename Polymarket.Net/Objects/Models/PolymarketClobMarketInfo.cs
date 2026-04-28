using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// V2 CLOB market info
    /// </summary>
    public record PolymarketClobMarketInfo
    {
        /// <summary>
        /// Tokens
        /// </summary>
        [JsonPropertyName("t")]
        public PolymarketClobMarketToken[] Tokens { get; set; } = [];

        /// <summary>
        /// Minimum order size
        /// </summary>
        [JsonPropertyName("mos")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal MinimumOrderSize { get; set; }

        /// <summary>
        /// Minimum tick size
        /// </summary>
        [JsonPropertyName("mts")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal MinimumTickSize { get; set; }

        /// <summary>
        /// Fee details
        /// </summary>
        [JsonPropertyName("fd")]
        public PolymarketClobMarketFeeDetails? FeeDetails { get; set; }

        /// <summary>
        /// Negative risk
        /// </summary>
        [JsonPropertyName("nr")]
        public bool NegativeRisk { get; set; }

        /// <summary>
        /// Maker base fee
        /// </summary>
        [JsonPropertyName("mbf")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal MakerBaseFee { get; set; }

        /// <summary>
        /// Taker base fee
        /// </summary>
        [JsonPropertyName("tbf")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal TakerBaseFee { get; set; }
    }

    /// <summary>
    /// V2 CLOB market token
    /// </summary>
    public record PolymarketClobMarketToken
    {
        /// <summary>
        /// Token id
        /// </summary>
        [JsonPropertyName("t")]
        public string TokenId { get; set; } = string.Empty;

        /// <summary>
        /// Outcome
        /// </summary>
        [JsonPropertyName("o")]
        public string Outcome { get; set; } = string.Empty;

        /// <summary>
        /// Price
        /// </summary>
        [JsonPropertyName("p")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Price { get; set; }

        /// <summary>
        /// Winner
        /// </summary>
        [JsonPropertyName("w")]
        public bool Winner { get; set; }
    }

    /// <summary>
    /// V2 CLOB fee details
    /// </summary>
    public record PolymarketClobMarketFeeDetails
    {
        /// <summary>
        /// Fee rate
        /// </summary>
        [JsonPropertyName("r")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Rate { get; set; }

        /// <summary>
        /// Fee exponent
        /// </summary>
        [JsonPropertyName("e")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Exponent { get; set; } = 1m;

        /// <summary>
        /// Fee recipient
        /// </summary>
        [JsonPropertyName("to")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string To { get; set; } = string.Empty;
    }

    internal sealed class FlexibleStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return string.Empty;

            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString() ?? string.Empty;

            using var document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.GetRawText();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
