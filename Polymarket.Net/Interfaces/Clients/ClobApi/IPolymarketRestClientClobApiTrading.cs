using CryptoExchange.Net.Objects;
using Polymarket.Net.Enums;
using Polymarket.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polymarket.Net.Interfaces.Clients.ClobApi
{
    /// <summary>
    /// Polymarket Clob trading endpoints, placing and managing orders.
    /// </summary>
    public interface IPolymarketRestClientClobApiTrading
    {
        Task<WebCallResult<PolymarketPage<PolymarketOrder>>> GetOpenOrdersAsync(string? orderId = null, string? marketId = null, string? assetId = null, CancellationToken ct = default);

        Task<WebCallResult<PolymarketOrderResult>> PlaceOrderAsync(
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
            CancellationToken ct = default);

        Task<WebCallResult<PolymarketCancelResult>> CancelOrderAsync(string orderId, CancellationToken ct = default);

        Task<WebCallResult<PolymarketCancelResult>> CancelOrdersAsync(IEnumerable<string> orderIds, CancellationToken ct = default);
        Task<WebCallResult<PolymarketCancelResult>> CancelOrdersOnMarketAsync(string? market = null, string? assetId = null, CancellationToken ct = default);
        Task<WebCallResult<PolymarketCancelResult>> CancelAllOrdersAsync(CancellationToken ct = default);
    }
}
