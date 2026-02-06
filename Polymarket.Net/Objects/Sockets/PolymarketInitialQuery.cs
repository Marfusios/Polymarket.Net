using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using Polymarket.Net.Objects.Internal;
using Polymarket.Net.Objects.Models;
using System;

namespace Polymarket.Net.Objects.Sockets
{
    internal class PolymarketInitialQuery<T> : Query<T>
    {
        public PolymarketInitialQuery(
            string type,
            string? key = null,
            string? sec = null,
            string? pass = null,
            string[]? assets = null) : base(new PolymarketSocketInitialRequest
        {
            Type = type,
            Assets = assets,
            CustomFeatureEnabled = string.Equals(type, "MARKET", StringComparison.OrdinalIgnoreCase)
                ? true
                : key != null ? true : null,
            Auth = key == null ? null : new PolymarketSocketAuth
            {
                ApiKey = key,
                ApiPass = pass!,
                ApiSecret = sec!
            }
        }, false, 1)
        {
            ExpectsResponse = false;

            MessageRouter = MessageRouter.CreateWithoutHandler<T>("");
        }
    }
}
