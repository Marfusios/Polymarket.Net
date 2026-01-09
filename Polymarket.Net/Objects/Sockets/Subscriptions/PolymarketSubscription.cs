using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using System;

namespace Polymarket.Net.Objects.Sockets.Subscriptions
{
    /// <inheritdoc />
    internal class PolymarketSubscription<T> : Subscription
    {
        private readonly Action<DateTime, string?, T> _handler;
        private readonly string[] _assetIds;

        /// <summary>
        /// ctor
        /// </summary>
        public PolymarketSubscription(ILogger logger, string[] assetIds, Action<DateTime, string?, T> handler, bool auth) : base(logger, auth)
        {
            _handler = handler;
            _assetIds = assetIds;

            MessageRouter = MessageRouter.CreateWithoutTopicFilter<T>(_assetIds, DoHandleMessage);
        }

        /// <inheritdoc />
        protected override Query? GetSubQuery(SocketConnection connection)
        {
            return new PolymarketQuery<object>("subscribe", _assetIds);
        }

        /// <inheritdoc />
        protected override Query? GetUnsubQuery(SocketConnection connection)
        {
            return new PolymarketQuery<object>("unsubscribe", _assetIds);
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, T message)
        {
            _handler.Invoke(receiveTime, originalData, message);
            return new CallResult(null);
        }
    }
}
