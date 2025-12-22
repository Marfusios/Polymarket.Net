using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using Polymarket.Net.Clients.MessageHandlers;
using Polymarket.Net.Interfaces.Clients.ClobApi;
using Polymarket.Net.Objects;
using Polymarket.Net.Objects.Models;
using Polymarket.Net.Objects.Options;
using Polymarket.Net.Objects.Sockets.Subscriptions;

namespace Polymarket.Net.Clients.ClobApi
{
    /// <summary>
    /// Client providing access to the Polymarket Clob websocket Api
    /// </summary>
    internal partial class PolymarketSocketClientClobApi : SocketApiClient, IPolymarketSocketClientClobApi
    {
        #region fields
        private static readonly MessagePath _idPath = MessagePath.Get().Property("id");

        protected override ErrorMapping ErrorMapping => PolymarketErrors.Errors;
        #endregion

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal PolymarketSocketClientClobApi(ILogger logger, PolymarketSocketOptions options) :
            base(logger, options.Environment.SocketClientAddress!, options, options.ClobOptions)
        {
        }
        #endregion

        /// <inheritdoc />
        protected override IByteMessageAccessor CreateAccessor(WebSocketMessageType type) => new SystemTextJsonByteMessageAccessor(PolymarketExchange._serializerContext);
        /// <inheritdoc />
        protected override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(PolymarketExchange._serializerContext);
        /// <inheritdoc />
        public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType) => new PolymarketSocketSpotMessageHandler();

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new PolymarketAuthenticationProvider((PolymarketCredentials)credentials);

        /// <inheritdoc />
        public async Task<CallResult<UpdateSubscription>> SubscribeToXXXUpdatesAsync(Action<DataEvent<PolymarketModel>> onMessage, CancellationToken ct = default)
        {
            var internalHandler = new Action<DateTime, string?, PolymarketModel>((receiveTime, originalData, data) =>
            {
                onMessage(
                    new DataEvent<PolymarketModel>(PolymarketExchange.ExchangeName, data, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        //.WithStreamId(data.Stream)
                        //.WithSymbol(data.Symbol)
                        //.WithDataTimestamp(data.EventTime)
                    );
            });

            var subscription = new PolymarketSubscription<PolymarketModel>(_logger, new [] { "XXX" }, internalHandler, false);
            return await SubscribeAsync(subscription, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override string? GetListenerIdentifier(IMessageAccessor message)
        {
            return message.GetValue<string>(_idPath);
        }

        /// <inheritdoc />
        protected override Task<Query?> GetAuthenticationRequestAsync(SocketConnection connection) => Task.FromResult<Query?>(null);


        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null)
            => PolymarketExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverDate);
    }
}
