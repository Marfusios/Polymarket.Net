using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Polymarket.Net.Objects.Internal;

namespace Polymarket.Net.Objects.Sockets
{
    internal class PolymarketUserQuery<T> : Query<T>
    {
        public PolymarketUserQuery(string operation, string[] markets) : base(new PolymarketUserSocketRequest
        {
            Operation = operation,
            Markets = markets
        }, false, 1)
        {
            ExpectsResponse = false;

            MessageRouter = MessageRouter.CreateWithoutHandler<T>("");
        }
    }
}
