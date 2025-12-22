using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using System;
using Polymarket.Net.Objects.Models;

namespace Polymarket.Net.Objects.Sockets
{
    internal class PolymarketQuery<T> : Query<T>
    {
        public PolymarketQuery(PolymarketModel request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
            MessageRouter = MessageRouter.CreateWithoutTopicFilter<T>("", HandleMessage);
        }

        public CallResult<T> HandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, T message)
        {
            return new CallResult<T>(message, originalData, null);
        }
    }
}
