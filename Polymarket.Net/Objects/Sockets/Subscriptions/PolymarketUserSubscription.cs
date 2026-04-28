using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using Polymarket.Net.Clients.ClobApi;
using Polymarket.Net.Objects;
using Polymarket.Net.Objects.Models;
using System;
using System.Linq;

namespace Polymarket.Net.Objects.Sockets.Subscriptions
{
    /// <inheritdoc />
    internal class PolymarketUserSubscription : Subscription
    {
        private readonly Action<DataEvent<PolymarketOrderUpdate>>? _orderUpdate;
        private readonly Action<DataEvent<PolymarketTradeUpdate>>? _tradeUpdate;
        private readonly string[] _marketIds;

        private PolymarketSocketClientClobApi _client;

        /// <summary>
        /// ctor
        /// </summary>
        public PolymarketUserSubscription(
            ILogger logger,
            PolymarketSocketClientClobApi client,
            Action<DataEvent<PolymarketOrderUpdate>>? orderUpdate,
            Action<DataEvent<PolymarketTradeUpdate>>? tradeUpdate,
            string[]? marketIds
            ) : base(logger, false)
        {
            _client = client;
            _orderUpdate = orderUpdate;
            _tradeUpdate = tradeUpdate;
            _marketIds = marketIds?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];

            IndividualSubscriptionCount = Math.Max(1, _marketIds.Length);

            MessageRouter = MessageRouter.Create([
                MessageRoute<PolymarketTradeUpdate>.CreateWithoutTopicFilter("trade", DoHandleMessage),
                MessageRoute<PolymarketOrderUpdate>.CreateWithoutTopicFilter("order", DoHandleMessage)
                ]);
        }

        /// <inheritdoc />
        protected override Query? GetSubQuery(SocketConnection connection)
        {
            var credentials = _client.ApiCredentials as PolymarketCredentials;
            if (credentials?.L2ApiKey == null)
                throw new InvalidOperationException("Layer 2 credentials required");

            return new PolymarketInitialQuery<object>(
                "user",
                credentials.L2ApiKey,
                credentials.L2Secret!,
                credentials.L2Pass!,
                assets: [],
                markets: _marketIds,
                operation: "subscribe",
                initialDump: true);
        }

        /// <inheritdoc />
        protected override Query? GetUnsubQuery(SocketConnection connection) => null;

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, PolymarketTradeUpdate message)
        {
            _client.UpdateTimeOffset(message.Timestamp);

            _tradeUpdate?.Invoke(new DataEvent<PolymarketTradeUpdate>(PolymarketPlatform.Metadata.Id, message, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(message.EventType)
                        //.WithSymbol(data.Symbol)
                        .WithDataTimestamp(message.Timestamp, _client.GetTimeOffset()));
            return new CallResult(null);
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, PolymarketOrderUpdate message)
        {
            _client.UpdateTimeOffset(message.Timestamp);

            _orderUpdate?.Invoke(new DataEvent<PolymarketOrderUpdate>(PolymarketPlatform.Metadata.Id, message, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(message.EventType)
                        //.WithSymbol(data.Symbol)
                        .WithDataTimestamp(message.Timestamp, _client.GetTimeOffset()));
            return new CallResult(null);
        }
    }
}
