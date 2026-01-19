using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using System;
using Polymarket.Net.Interfaces;
using Polymarket.Net.Objects.Options;
using CryptoExchange.Net.OrderBook;
using Microsoft.Extensions.DependencyInjection;
using Polymarket.Net.Interfaces.Clients;
using Microsoft.Extensions.Logging;

namespace Polymarket.Net.SymbolOrderBooks
{
    /// <summary>
    /// Polymarket order book factory
    /// </summary>
    public class PolymarketOrderBookFactory : IPolymarketOrderBookFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving logging and clients</param>
        public PolymarketOrderBookFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

         /// <inheritdoc />
        public ISymbolOrderBook CreateClob(string tokenId, Action<PolymarketOrderBookOptions>? options = null)
            => new PolymarketClobSymbolOrderBook(tokenId, options, 
                                                          _serviceProvider.GetRequiredService<ILoggerFactory>(),
                                                          _serviceProvider.GetRequiredService<IPolymarketRestClient>(),
                                                          _serviceProvider.GetRequiredService<IPolymarketSocketClient>());


    }
}
