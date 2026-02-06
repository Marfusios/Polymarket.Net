using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using Polymarket.Net.Clients.ClobApi;
using Polymarket.Net.Objects.Models;
using System;

namespace Polymarket.Net.Objects.Sockets.Subscriptions
{
    /// <inheritdoc />
    internal class PolymarketGeneralSystemSubscription : SystemSubscription
    {
        /// <summary>
        /// ctor
        /// </summary>
        public PolymarketGeneralSystemSubscription(ILogger logger) : base(logger, false)
        {
            MessageRouter = MessageRouter.Create([
                MessageRoute<PolymarketNewMarketUpdate>.CreateWithoutTopicFilter("new_market", DoHandleMessage),
                MessageRoute<PolymarketMarketResolvedUpdate>.CreateWithoutTopicFilter("market_resolved", DoHandleMessage)
                ]);
        }

        /// <inheritdoc />
        protected override Query? GetSubQuery(SocketConnection connection)
        {
            return new PolymarketInitialQuery<object>("MARKET");
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, PolymarketNewMarketUpdate message)
        {
            return CallResult.SuccessResult;
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, PolymarketMarketResolvedUpdate message)
        {
            return CallResult.SuccessResult;
        }
    }
}
