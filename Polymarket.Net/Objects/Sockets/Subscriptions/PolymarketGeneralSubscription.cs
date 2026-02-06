using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using Polymarket.Net.Clients.ClobApi;
using Polymarket.Net.Objects.Models;
using System;
using System.Linq;

namespace Polymarket.Net.Objects.Sockets.Subscriptions
{
    /// <inheritdoc />
    internal class PolymarketGeneralSubscription : Subscription
    {
        private readonly Action<DataEvent<PolymarketNewMarketUpdate>>? _newMarketUpdate;
        private readonly Action<DataEvent<PolymarketMarketResolvedUpdate>>? _marketResolvedUpdate;
        private readonly string[] _assetIds;

        private PolymarketSocketClientClobApi _client;

        /// <summary>
        /// ctor
        /// </summary>
        public PolymarketGeneralSubscription(
            ILogger logger,
            PolymarketSocketClientClobApi client,
            string[]? assetIds,
            Action<DataEvent<PolymarketNewMarketUpdate>>? newMarketUpdate,
            Action<DataEvent<PolymarketMarketResolvedUpdate>>? marketResolvedUpdate
            ) : base(logger, false)
        {
            _client = client;
            _assetIds = assetIds?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();
            _newMarketUpdate = newMarketUpdate;
            _marketResolvedUpdate = marketResolvedUpdate;

            MessageRouter = MessageRouter.Create([
                MessageRoute<PolymarketNewMarketUpdate>.CreateWithoutTopicFilter("new_market", DoHandleMessage),
                MessageRoute<PolymarketMarketResolvedUpdate>.CreateWithoutTopicFilter("market_resolved", DoHandleMessage)
                ]);
        }

        /// <inheritdoc />
        protected override Query? GetSubQuery(SocketConnection connection)
        {
            return new PolymarketInitialQuery<object>(
                "MARKET",
                assets: _assetIds.Length == 0 ? null : _assetIds);
        }

        /// <inheritdoc />
        protected override Query? GetUnsubQuery(SocketConnection connection) => null;

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, PolymarketNewMarketUpdate message)
        {
            _client.UpdateTimeOffset(message.Timestamp);

            _newMarketUpdate?.Invoke(new DataEvent<PolymarketNewMarketUpdate>(PolymarketPlatform.Metadata.Id, message, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(message.EventType)
                        //.WithSymbol(data.Symbol)
                        .WithDataTimestamp(message.Timestamp, _client.GetTimeOffset()));
            return new CallResult(null);
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, PolymarketMarketResolvedUpdate message)
        {
            _client.UpdateTimeOffset(message.Timestamp);

            _marketResolvedUpdate?.Invoke(new DataEvent<PolymarketMarketResolvedUpdate>(PolymarketPlatform.Metadata.Id, message, receiveTime, originalData)
                        .WithUpdateType(SocketUpdateType.Update)
                        .WithStreamId(message.EventType)
                        //.WithSymbol(data.Symbol)
                        .WithDataTimestamp(message.Timestamp, _client.GetTimeOffset()));
            return new CallResult(null);
        }
    }
}
