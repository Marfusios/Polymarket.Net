using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using System.Text.Json;
using CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers;

namespace Polymarket.Net.Clients.MessageHandlers
{
    internal class PolymarketSocketSpotMessageHandler : JsonSocketMessageHandler
    {
        public override JsonSerializerOptions Options { get; } = PolymarketExchange._serializerContext;

        public PolymarketSocketSpotMessageHandler()
        {
        }

        protected override MessageTypeDefinition[] TypeEvaluators { get; } = [

            //new MessageTypeDefinition {
            //    Fields = [
            //        new PropertyFieldReference("stream"),
            //    ],
            //    TypeIdentifierCallback = x => x.FieldValue("stream")!,
            //},

            //new MessageTypeDefinition {
            //    ForceIfFound = true,
            //    Fields = [
            //        new PropertyFieldReference("id"),
            //    ],
            //    TypeIdentifierCallback = x => x.FieldValue("id")!,
            //}
        ];
    }
}
