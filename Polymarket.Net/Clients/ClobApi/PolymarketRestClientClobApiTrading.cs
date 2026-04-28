using CryptoExchange.Net;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using Polymarket.Net.Enums;
using Polymarket.Net.Interfaces.Clients.ClobApi;
using Polymarket.Net.Objects;
using Polymarket.Net.Objects.Models;
using Polymarket.Net.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Polymarket.Net.Clients.ClobApi
{
    /// <inheritdoc />
    internal class PolymarketRestClientClobApiTrading : IPolymarketRestClientClobApiTrading
    {
        private const string ZeroBytes32 = "0x0000000000000000000000000000000000000000000000000000000000000000";
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly PolymarketRestClientClobApi _baseClient;
        private readonly ILogger _logger;

        internal PolymarketRestClientClobApiTrading(ILogger logger, PolymarketRestClientClobApi baseClient)
        {
            _baseClient = baseClient;
            _logger = logger;
        }

        public async Task<WebCallResult<PolymarketOrderResult>> PlaceOrderAsync(
            string tokenId,
            OrderSide side,
            OrderType orderType,
            decimal quantity,
            decimal? price = null,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            long? clientOrderId = null,
            DateTime? expiration = null,
            string? metadata = null,
            string? builderCode = null,
            bool? deferExecution = null,
            CancellationToken ct = default)
        {
            var tokenResult = await PolymarketUtils.GetTokenInfoAsync(tokenId, _baseClient, ct).ConfigureAwait(false);
            if (!tokenResult)
                return new WebCallResult<PolymarketOrderResult>(tokenResult.Error);

            var makerTakerQuantities = await GetMakerTakerQuantitiesAsync(tokenId, side, orderType, quantity, price, timeInForce).ConfigureAwait(false);
            if (!makerTakerQuantities)
                return new WebCallResult<PolymarketOrderResult>(makerTakerQuantities.Error);

            var parameters = new ParameterCollection();
            var authProvider = (PolymarketAuthenticationProvider)_baseClient.AuthenticationProvider!;
            var orderParameters = BuildV2OrderParameters(
                authProvider,
                tokenId,
                side,
                makerTakerQuantities.Data.MakerQuantity,
                makerTakerQuantities.Data.TakerQuantity,
                clientOrderId,
                expiration,
                metadata,
                builderCode ?? _baseClient.ClientOptions.BuilderCode);

            orderParameters.Add(
                "signature",
                authProvider.GetOrderSignature(
                    orderParameters,
                    _baseClient.ClientOptions.Environment.ChainId,
                    tokenResult.Data.NegativeRisk).ToLowerInvariant());

            parameters.Add("order", orderParameters);
            parameters.Add("owner", authProvider.ApiKey);
            parameters.AddEnum("orderType", timeInForce ?? TimeInForce.GoodTillCanceled);
            parameters.Add("postOnly", postOnly ?? false);
            parameters.Add("deferExec", deferExecution ?? false);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "/order", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(3500, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketOrderResult>(request, parameters, ct).ConfigureAwait(false);

            if (!result)
                return result;

            if (!string.IsNullOrEmpty(result.Data.Error))
                return result.AsError<PolymarketOrderResult>(new ServerError(_baseClient.GetErrorInfo(result.Data.Error!, result.Data.Error)));

            return result;
        }

        public async Task<WebCallResult<CallResult<PolymarketOrderResult>[]>> PlaceMultipleOrdersAsync(IEnumerable<PolymarketOrderRequest> requests, CancellationToken ct = default)
        {
            var parameterList = new List<ParameterCollection>();
            foreach (var request in requests)
            {
                var tokenResult = await PolymarketUtils.GetTokenInfoAsync(request.TokenId, _baseClient, ct).ConfigureAwait(false);
                if (!tokenResult)
                    return new WebCallResult<CallResult<PolymarketOrderResult>[]>(tokenResult.Error);

                var makerTakerQuantities = await GetMakerTakerQuantitiesAsync(request.TokenId, request.Side, request.OrderType, request.Quantity, request.Price, request.TimeInForce).ConfigureAwait(false);
                if (!makerTakerQuantities)
                    return new WebCallResult<CallResult<PolymarketOrderResult>[]>(makerTakerQuantities.Error);

                var parameters = new ParameterCollection();
                var authProvider = (PolymarketAuthenticationProvider)_baseClient.AuthenticationProvider!;
                var orderParameters = BuildV2OrderParameters(
                    authProvider,
                    request.TokenId,
                    request.Side,
                    makerTakerQuantities.Data.MakerQuantity,
                    makerTakerQuantities.Data.TakerQuantity,
                    request.ClientOrderId,
                    request.Expiration,
                    request.Metadata,
                    request.BuilderCode ?? _baseClient.ClientOptions.BuilderCode);

                orderParameters.Add("signature",
                    authProvider.GetOrderSignature(
                        orderParameters,
                        _baseClient.ClientOptions.Environment.ChainId,
                        tokenResult.Data.NegativeRisk).ToLowerInvariant());

                parameters.Add("order", orderParameters);
                parameters.Add("owner", authProvider.ApiKey);
                parameters.AddEnum("orderType", request.TimeInForce ?? TimeInForce.GoodTillCanceled);
                parameters.Add("postOnly", request.PostOnly ?? false);
                parameters.Add("deferExec", request.DeferExecution ?? false);
                parameterList.Add(parameters);
            }

            var requestParams = new ParameterCollection();
            requestParams.SetBody(parameterList.ToArray());
            var requestDef = _definitions.GetOrCreate(HttpMethod.Post, "/orders", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(1000, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketOrderResult[]>(requestDef, requestParams, ct).ConfigureAwait(false);
            if (!result)
                return result.As<CallResult<PolymarketOrderResult>[]>(default);

            var ordersResult = new List<CallResult<PolymarketOrderResult>>();
            foreach (var item in result.Data)
            {
                if (!string.IsNullOrEmpty(item.Error))
                    ordersResult.Add(new CallResult<PolymarketOrderResult>(item, null, new ServerError(_baseClient.GetErrorInfo(item.Error!, item.Error))));
                else
                    ordersResult.Add(new CallResult<PolymarketOrderResult>(item));
            }

            if (ordersResult.All(x => !x.Success))
                return result.AsErrorWithData(new ServerError(new ErrorInfo(ErrorType.AllOrdersFailed, "All orders failed")), ordersResult.ToArray());

            return result.As(ordersResult.ToArray());
        }

        private static ParameterCollection BuildV2OrderParameters(
            PolymarketAuthenticationProvider authProvider,
            string tokenId,
            OrderSide side,
            decimal makerQuantity,
            decimal takerQuantity,
            long? clientOrderId,
            DateTime? expiration,
            string? metadata,
            string? builderCode)
        {
            var orderParameters = new ParameterCollection();
            orderParameters.Add("salt", (ulong)(clientOrderId ?? ExchangeHelpers.RandomLong(1000000000000, 9999999999999)));
            orderParameters.Add("maker", authProvider.PolymarketFundingAddress ?? authProvider.PublicAddress);
            orderParameters.Add("signer", authProvider.PublicAddress);
            orderParameters.Add("tokenId", tokenId);
            orderParameters.AddString("makerAmount", makerQuantity);
            orderParameters.AddString("takerAmount", takerQuantity);
            orderParameters.AddEnum("side", side);
            orderParameters.Add("signatureType", (int)authProvider.SignatureType);
            orderParameters.AddString("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            orderParameters.Add("metadata", NormalizeBytes32(metadata, nameof(metadata)));
            orderParameters.Add("builder", NormalizeBytes32(builderCode, nameof(builderCode)));
            orderParameters.AddString("expiration", (ulong)(expiration == null ? 0 : DateTimeConverter.ConvertToSeconds(expiration.Value)));
            return orderParameters;
        }

        internal static string NormalizeBytes32(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ZeroBytes32;

            var normalized = value.Trim();
            if (normalized.Length != 66 || !normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"{parameterName} must be a 0x-prefixed bytes32 hex value", parameterName);

            for (var i = 2; i < normalized.Length; i++)
            {
                if (!Uri.IsHexDigit(normalized[i]))
                    throw new ArgumentException($"{parameterName} must be a 0x-prefixed bytes32 hex value", parameterName);
            }

            return normalized.ToLowerInvariant();
        }

        private async Task<CallResult<(decimal MakerQuantity, decimal TakerQuantity)>> GetMakerTakerQuantitiesAsync(string tokenId, OrderSide side, OrderType orderType, decimal quantity, decimal? price, TimeInForce? timeInForce)
        {
            decimal takerQuantity;
            decimal makerQuantity;
            if (orderType == OrderType.Limit)
            {
                if (price == null)
                    throw new ArgumentNullException(nameof(price), "Price is required for limit orders");
            }
            else
            {
                var bookInfo = await _baseClient.ExchangeData.GetOrderBookAsync(tokenId).ConfigureAwait(false);
                if (!bookInfo)
                    return bookInfo.As<(decimal, decimal)>(default);

                if (side == OrderSide.Buy)
                {
                    decimal? marketPrice = null;
                    var sum = 0m;
                    for (var i = bookInfo.Data.Asks.Length - 1; i >= 0; i--)
                    {
                        var ask = bookInfo.Data.Asks[i];
                        sum += ask.Quantity * ask.Price;
                        if (sum >= quantity)
                        {
                            marketPrice = ask.Price;
                            break;
                        }
                    }

                    if (timeInForce == TimeInForce.FillOrKill && marketPrice == null)
                        return new WebCallResult<(decimal, decimal)>(new ServerError(new ErrorInfo(ErrorType.RejectedOrderConfiguration, "FOK order couldn't fill")));

                    price = marketPrice ?? bookInfo.Data.Asks[0].Price;
                }
                else
                {
                    decimal? marketPrice = null;
                    var sum = 0m;
                    for (var i = bookInfo.Data.Bids.Length - 1; i >= 0; i--)
                    {
                        var bid = bookInfo.Data.Bids[i];
                        sum += bid.Quantity;
                        if (sum >= quantity)
                        {
                            marketPrice = bid.Price;
                            break;
                        }
                    }

                    if (timeInForce == TimeInForce.FillOrKill && marketPrice == null)
                        return new WebCallResult<(decimal, decimal)>(new ServerError(new ErrorInfo(ErrorType.RejectedOrderConfiguration, "FOK order couldn't fill")));

                    price = marketPrice ?? bookInfo.Data.Bids[0].Price;
                }

            }

            price = NormalizeOrderPrice(price!.Value);

            if (side == OrderSide.Buy)
            {
                takerQuantity = ExchangeHelpers.RoundDown(quantity, 2);
                makerQuantity = takerQuantity * price.Value;
            }
            else
            {
                makerQuantity = ExchangeHelpers.RoundDown(quantity, 2);
                takerQuantity = makerQuantity * price.Value;
            }

            // Preserve sub-cent precision in taker amount so maker/taker ratio remains on valid tick after conversion.
            takerQuantity = ExchangeHelpers.RoundDown(takerQuantity, 6);

            takerQuantity = ConvertToClobBaseUnits(takerQuantity);
            makerQuantity = ConvertToClobBaseUnits(makerQuantity);

            takerQuantity = takerQuantity.Normalize();
            makerQuantity = makerQuantity.Normalize();

            return new CallResult<(decimal, decimal)>((makerQuantity, takerQuantity));
        }

        internal static decimal NormalizeOrderPrice(decimal price)
        {
            return Math.Round(price, 3).Normalize();
        }

        internal static decimal ConvertToClobBaseUnits(decimal amount)
        {
            return Math.Truncate(amount * 1000000m);
        }

        public async Task<WebCallResult<PolymarketOrder>> GetOrderAsync(string orderId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/data/order/" + orderId, PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(900, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketOrder>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketPage<PolymarketOrder>>> GetOpenOrdersAsync(string? orderId = null, string? conditionId = null, string? assetId = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/data/orders", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(500, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketPage<PolymarketOrder>>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketOrderScoring>> GetOrderRewardScoringAsync(string orderId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("order_id", orderId);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/order-scoring", PolymarketPlatform.RateLimiter.ClobApi, 1, true);
            var result = await _baseClient.SendAsync<PolymarketOrderScoring>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }


        public async Task<WebCallResult<Dictionary<string, bool>>> GetOrdersRewardScoringAsync(IEnumerable<string> orderIds, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.SetBody(orderIds.ToArray());
            var request = _definitions.GetOrCreate(HttpMethod.Post, "/orders-scoring", PolymarketPlatform.RateLimiter.ClobApi, 1, true);
            var result = await _baseClient.SendAsync<Dictionary<string, bool>>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketHeartbeatResult>> PostHeartbeatAsync(string? market = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("market", market);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "/heartbeat", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(500, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketHeartbeatResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCancelResult>> CancelOrderAsync(string orderId, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.Add("orderID", orderId);
            var request = _definitions.GetOrCreate(HttpMethod.Delete, "/order", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(3000, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketCancelResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCancelResult>> CancelOrdersAsync(IEnumerable<string> orderIds, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.SetBody(orderIds.ToArray());
            var request = _definitions.GetOrCreate(HttpMethod.Delete, "/orders", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(1000, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketCancelResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCancelResult>> CancelOrdersOnMarketAsync(string? market = null, string? tokenId = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("market", market);
            parameters.AddOptional("asset_id", tokenId);
            var request = _definitions.GetOrCreate(HttpMethod.Delete, "/orders", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(1000, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketCancelResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCancelResult>> CancelAllOrdersAsync(CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            var request = _definitions.GetOrCreate(HttpMethod.Delete, "/cancel-all", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(250, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketCancelResult>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketPage<PolymarketTrade>>> GetUserTradesAsync(
            string? tradeId = null,
            string? takerAddress = null,
            string? makerAddress = null,
            string? conditionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            string? cursor = null,
            CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("id", cursor);
            parameters.AddOptional("taker", cursor);
            parameters.AddOptional("maker", cursor);
            parameters.AddOptional("market", cursor);
            parameters.AddOptionalMillisecondsString("after", startTime);
            parameters.AddOptionalMillisecondsString("before", endTime);
            parameters.AddOptional("next_cursor", cursor);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "/data/trades", PolymarketPlatform.RateLimiter.ClobApi, 1, true,
                limitGuard: new SingleLimitGuard(500, TimeSpan.FromSeconds(10), RateLimitWindowType.Sliding));
            var result = await _baseClient.SendAsync<PolymarketPage<PolymarketTrade>>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

    }
}
