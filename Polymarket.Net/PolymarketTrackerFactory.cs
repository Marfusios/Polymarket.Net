using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Klines;
using CryptoExchange.Net.Trackers.Trades;
using Polymarket.Net.Interfaces;
using Polymarket.Net.Interfaces.Clients;
using Microsoft.Extensions.DependencyInjection;
using System;
using Polymarket.Net.Clients;

namespace Polymarket.Net
{
    /// <inheritdoc />
    public class PolymarketTrackerFactory : IPolymarketTrackerFactory
    {
        private readonly IServiceProvider? _serviceProvider;

        /// <summary>
        /// ctor
        /// </summary>
        public PolymarketTrackerFactory()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving logging and clients</param>
        public PolymarketTrackerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool CanCreateKlineTracker(SharedSymbol symbol, SharedKlineInterval interval)
        {
            var client = _serviceProvider?.GetRequiredService<IPolymarketSocketClient>() ?? new PolymarketSocketClient();
#warning TODO
            SubscribeKlineOptions klineOptions = new SubscribeKlineOptions(true);
            return klineOptions.IsSupported(interval); 
        }

        public bool CanCreateTradeTracker(SharedSymbol symbol) => true;

        /// <inheritdoc />
        public IKlineTracker CreateKlineTracker(SharedSymbol symbol, SharedKlineInterval interval, int? limit = null, TimeSpan? period = null)
        {
            var restClient = _serviceProvider?.GetRequiredService<IPolymarketRestClient>() ?? new PolymarketRestClient();
            var socketClient = _serviceProvider?.GetRequiredService<IPolymarketSocketClient>() ?? new PolymarketSocketClient();

#warning todo
            throw new NotImplementedException();
            //IKlineRestClient sharedRestClient;
            //IKlineSocketClient sharedSocketClient;
            //if (symbol.TradingMode == TradingMode.Spot)
            //{
            //    sharedRestClient = restClient.SpotApi.SharedClient;
            //    sharedSocketClient = socketClient.SpotApi.SharedClient;
            //}
            //else
            //{
            //    sharedRestClient = restClient.FuturesApi.SharedClient;
            //    sharedSocketClient = socketClient.FuturesApi.SharedClient;
            //}

            //return new KlineTracker(
            //    _serviceProvider?.GetRequiredService<ILoggerFactory>().CreateLogger(restClient.Exchange),
            //    sharedRestClient,
            //    sharedSocketClient,
            //    symbol,
            //    interval,
            //    limit,
            //    period
            //    );
        }
        /// <inheritdoc />
        public ITradeTracker CreateTradeTracker(SharedSymbol symbol, int? limit = null, TimeSpan? period = null)
        {
            var restClient = _serviceProvider?.GetRequiredService<IPolymarketRestClient>() ?? new PolymarketRestClient();
            var socketClient = _serviceProvider?.GetRequiredService<IPolymarketSocketClient>() ?? new PolymarketSocketClient();

#warning todo
            throw new NotImplementedException();

            //IRecentTradeRestClient? sharedRestClient;
            //ITradeSocketClient sharedSocketClient;
            //if (symbol.TradingMode == TradingMode.Spot)
            //{
            //    sharedRestClient = restClient.SpotApi.SharedClient;
            //    sharedSocketClient = socketClient.SpotApi.SharedClient;
            //}
            //else
            //{
            //    sharedRestClient = restClient.FuturesApi.SharedClient;
            //    sharedSocketClient = socketClient.FuturesApi.SharedClient;
            //}

            //return new TradeTracker(
            //    _serviceProvider?.GetRequiredService<ILoggerFactory>().CreateLogger(restClient.Exchange),
            //    sharedRestClient,
            //    null,
            //    sharedSocketClient,
            //    symbol,
            //    limit,
            //    period
            //    );
        }
    }
}
