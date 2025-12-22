using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using Polymarket.Net.Enums;
using Polymarket.Net.Interfaces.Clients.ClobApi;
using Polymarket.Net.Objects.Models;
using Polymarket.Net.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Polymarket.Net.Clients.ClobApi
{
    /// <inheritdoc />
    internal class PolymarketRestClientClobApiTrading : IPolymarketRestClientClobApiTrading
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly PolymarketRestClientClobApi _baseClient;
        private readonly ILogger _logger;

        internal PolymarketRestClientClobApiTrading(ILogger logger, PolymarketRestClientClobApi baseClient)
        {
            _baseClient = baseClient;
            _logger = logger;
        }

        public async Task<WebCallResult<PolymarketPage<PolymarketOrder>>> GetOpenOrdersAsync(string? orderId = null, string? marketId = null, string? assetId = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/data/orders", PolymarketExchange.RateLimiter.Polymarket, 1, true);
            var result = await _baseClient.SendAsync<PolymarketPage<PolymarketOrder>>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketOrderResult>> PlaceOrderAsync(
            string tokenId,
            OrderSide side,
            TimeInForce timeInForce,
            decimal makerQuantity,
            decimal takerQuantity,
            long nonce,
            decimal feeRateBps,
            int signatureType,
            string? fundingAddress = null,
            string? signingAddress = null,
            string? operatorAddress = null,
            string? clientOrderId = null,
            DateTime? expiration = null,
            CancellationToken ct = default)
        {
            var tokenResult = await PolymarketUtils.GetTokenInfoAsync(tokenId, _baseClient).ConfigureAwait(false);
            if (!tokenResult)
                return new WebCallResult<PolymarketOrderResult>(tokenResult.Error);

            var tickSize = tokenResult.Data.TickQuantity;

            var parameters = new ParameterCollection();
            var orderParameters = new ParameterCollection();

            parameters.Add("order", orderParameters);
            parameters.Add("owner", _baseClient.AuthenticationProvider?.ApiKey);
            parameters.AddEnum("orderType", timeInForce);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "/order", PolymarketExchange.RateLimiter.Polymarket, 1, true);
            var result = await _baseClient.SendAsync<PolymarketOrderResult>(request, parameters, ct).ConfigureAwait(false);

            // If errorMsg is set return error
            return result;
        }

        public async Task<WebCallResult<PolymarketCancelResult>> CancelOrderAsync(string orderId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("orderID", orderId);
            var request = _definitions.GetOrCreate(HttpMethod.Delete, "/order", PolymarketExchange.RateLimiter.Polymarket, 1, true);
            var result = await _baseClient.SendAsync<PolymarketCancelResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCancelResult>> CancelOrdersAsync(IEnumerable<string> orderIds, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.SetBody(orderIds.ToArray());
            var request = _definitions.GetOrCreate(HttpMethod.Delete, "/orders", PolymarketExchange.RateLimiter.Polymarket, 1, true);
            var result = await _baseClient.SendAsync<PolymarketCancelResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCancelResult>> CancelOrdersOnMarketAsync(string? market = null, string? assetId = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("market", market);
            parameters.AddOptional("asset_id", assetId);
            var request = _definitions.GetOrCreate(HttpMethod.Delete, "/orders", PolymarketExchange.RateLimiter.Polymarket, 1, true);
            var result = await _baseClient.SendAsync<PolymarketCancelResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCancelResult>> CancelAllOrdersAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Delete, "/cancel-all", PolymarketExchange.RateLimiter.Polymarket, 1, true);
            var result = await _baseClient.SendAsync<PolymarketCancelResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }
    }
}
