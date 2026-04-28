using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polymarket.Net.Converters
{
    /// <summary>
    /// Reads a JSON decimal that may also arrive as a string. Treats `""` and `null`
    /// as zero so per-maker-order entries from the WS user channel (which omit the
    /// fee value with an empty string) don't fail whole-message deserialization.
    /// </summary>
    internal class PolymarketEmptyStringDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return 0m;

            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetDecimal();

            if (reader.TokenType == JsonTokenType.String)
            {
                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text))
                    return 0m;
                return decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when reading decimal");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
