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

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="topics"></param>
        /// <param name="handler"></param>
        /// <param name="auth"></param>
        public PolymarketSubscription(ILogger logger, string[] topics, Action<DateTime, string?, T> handler, bool auth) : base(logger, auth)
        {
            _handler = handler;

            MessageRouter = MessageRouter.CreateWithoutTopicFilter<T>(topics, DoHandleMessage);
        }

        /// <inheritdoc />
        protected override Query? GetSubQuery(SocketConnection connection)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override Query? GetUnsubQuery(SocketConnection connection)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, T message)
        {
            _handler.Invoke(receiveTime, originalData, message);
            return new CallResult(null);
        }
    }
}
