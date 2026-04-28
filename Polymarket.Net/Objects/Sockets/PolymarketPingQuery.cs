using CryptoExchange.Net.Sockets;
using System;

namespace Polymarket.Net.Objects.Sockets
{
    internal class PolymarketPingQuery : Query<string>
    {
        public PolymarketPingQuery() : base("PING", false, 0)
        {
            RequestTimeout = TimeSpan.FromSeconds(5);
            MessageRouter = MessageRouter.CreateWithoutHandler<string>("PONG");
        }
    }
}
