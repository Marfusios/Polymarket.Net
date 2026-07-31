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
        /// <summary>
        /// Get open orders
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/get-active-order" /></para>
        /// </summary>
        /// <param name="orderId">Filter by order id</param>
        /// <param name="conditionId">Filter by market/condition id</param>
        /// <param name="tokenId">Asset/token id</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketPage<PolymarketOrder>>> GetOpenOrdersAsync(string? orderId = null, string? conditionId = null, string? tokenId = null, CancellationToken ct = default);

        /// <summary>
        /// Get an order by id
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/get-order" /></para>
        /// </summary>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketOrder>> GetOrderAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Check if an order is eligible or scoring for Rewards purposes
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/check-scoring" /></para>
        /// </summary>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketOrderScoring>> GetOrderRewardScoringAsync(string orderId, CancellationToken ct = default);

        /// <summary>
        /// Check if orders are eligible or scoring for Rewards purposes
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/check-scoring" /></para>
        /// </summary>
        /// <param name="orderIds">Order ids</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<Dictionary<string, bool>>> GetOrdersRewardScoringAsync(IEnumerable<string> orderIds, CancellationToken ct = default);

        /// <summary>
        /// Post heartbeat to keep maker orders alive for the market
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/heartbeat" /></para>
        /// </summary>
        /// <param name="conditionId">Optional market/condition id</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketHeartbeatResult>> PostHeartbeatAsync(string? conditionId = null, CancellationToken ct = default);

        /// <summary>
        /// Place a new order
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/create-order" /></para>
        /// </summary>
        /// <param name="tokenId">Token id</param>
        /// <param name="side">Side</param>
        /// <param name="orderType">Type of order</param>
        /// <param name="timeInForce">Time in force</param>
        /// <param name="quantity">Quantity of shares</param>
        /// <param name="price">Price, value between 0 and 1. For example 0.001 means 0.1c in the UI, 0.5 means 50c in UI</param>
        /// <param name="postOnly">Post only order</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="expiration">Expiration time</param>
        /// <param name="metadata">Metadata bytes32</param>
        /// <param name="builderCode">Builder attribution code bytes32</param>
        /// <param name="deferExecution">Defer execution</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketOrderResult>> PlaceOrderAsync(
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
            CancellationToken ct = default);

        /// <summary>
        /// Place multiple orders in a single request
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/create-order-batch" /></para>
        /// </summary>
        /// <param name="requests">Order requests</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<CallResult<PolymarketOrderResult>[]>> PlaceMultipleOrdersAsync(IEnumerable<PolymarketOrderRequest> requests, CancellationToken ct = default);

        /// <summary>
        /// Build and sign the order body synchronously (no HTTP). Returns a pre-signed envelope
        /// that can be passed to PlaceSignedOrderAsync to skip signing at placement time.
        /// Each PreSignedOrder may only be submitted once: the salt acts as a server-side nonce.
        /// </summary>
        /// <param name="tokenId">UP or DOWN token id</param>
        /// <param name="side">Buy/Sell</param>
        /// <param name="quantity">Quantity in shares (limit order)</param>
        /// <param name="price">Limit price (0..1)</param>
        /// <param name="negativeRisk">Whether the market is a negative-risk market (from getClobMarketInfo)</param>
        /// <param name="clientOrderId">Explicit salt (caller is responsible for uniqueness if provided)</param>
        /// <param name="expiration">Order expiration (null = GTC, expiration field signed as 0)</param>
        /// <param name="metadata">Bytes32 metadata; null becomes 0x00..</param>
        /// <param name="builderCode">Bytes32 builder attribution; null falls back to client default</param>
        /// <param name="isWideLimit">If true, mark the resulting order as a "wide-limit" boot entry.
        /// Affects only client-side cache behavior; the signed payload is unchanged.</param>
        PreSignedOrder BuildAndSignOrder(
            string tokenId,
            OrderSide side,
            decimal quantity,
            decimal price,
            bool negativeRisk,
            long? clientOrderId = null,
            DateTime? expiration = null,
            string? metadata = null,
            string? builderCode = null,
            bool isWideLimit = false);

        /// <summary>
        /// Build and sign a marketable BUY from an explicit USDC maker amount. The maker
        /// amount is rounded down to cents and the resulting share amount to four decimals.
        /// This performs no HTTP request; submit the result with PlaceSignedOrderAsync.
        /// </summary>
        /// <param name="tokenId">UP or DOWN token id</param>
        /// <param name="makerNotionalUsd">Maximum USDC to spend</param>
        /// <param name="limitPrice">Worst accepted execution price (0..1)</param>
        /// <param name="negativeRisk">Whether the market is a negative-risk market</param>
        /// <param name="clientOrderId">Explicit unique salt</param>
        /// <param name="expiration">Order expiration (null signs zero)</param>
        /// <param name="metadata">Bytes32 metadata</param>
        /// <param name="builderCode">Bytes32 builder attribution</param>
        PreSignedOrder BuildAndSignMarketBuyOrder(
            string tokenId,
            decimal makerNotionalUsd,
            decimal limitPrice,
            bool negativeRisk,
            long? clientOrderId = null,
            DateTime? expiration = null,
            string? metadata = null,
            string? builderCode = null);

        /// <summary>
        /// Place a pre-built and pre-signed order. Skips quantity calc, parameter assembly,
        /// and EIP-712 signing — performs only the HTTP submit. Use the output of BuildAndSignOrder.
        /// </summary>
        /// <param name="signedOrder">Output of BuildAndSignOrder</param>
        /// <param name="timeInForce">Time in force at place time</param>
        /// <param name="postOnly">Post-only flag</param>
        /// <param name="deferExecution">Defer execution flag</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketOrderResult>> PlaceSignedOrderAsync(
            PreSignedOrder signedOrder,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? deferExecution = null,
            CancellationToken ct = default);

        /// <summary>
        /// Cancel an order
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/cancel-orders" /></para>
        /// </summary>
        /// <param name="orderId">Order id</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketCancelResult>> CancelOrderAsync(string orderId, CancellationToken ct = default);
        /// <summary>
        /// Cancel multiple orders
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/cancel-orders" /></para>
        /// </summary>
        /// <param name="orderIds">Ids of orders to cancel</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketCancelResult>> CancelOrdersAsync(IEnumerable<string> orderIds, CancellationToken ct = default);
        /// <summary>
        /// Cancel all orders for a specific market
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/cancel-orders" /></para>
        /// </summary>
        /// <param name="conditionId">The condition/market id</param>
        /// <param name="tokenId">Asset/token id</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketCancelResult>> CancelOrdersOnMarketAsync(string? conditionId = null, string? tokenId = null, CancellationToken ct = default);
        /// <summary>
        /// Cancel all open orders
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/orders/cancel-orders" /></para>
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketCancelResult>> CancelAllOrdersAsync(CancellationToken ct = default);

        /// <summary>
        /// Get trades matching the filters
        /// <para><a href="https://docs.polymarket.com/developers/CLOB/trades/trades" /></para>
        /// </summary>
        /// <param name="tradeId">Filter by trade id</param>
        /// <param name="takerAddress">Filter by taker address</param>
        /// <param name="makerAddress">Filter by maker address</param>
        /// <param name="conditionId">Filter by condition id</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="cursor">Next page cursor</param>
        /// <param name="ct">Cancellation token</param>
        Task<WebCallResult<PolymarketPage<PolymarketTrade>>> GetUserTradesAsync(
            string? tradeId = null,
            string? takerAddress = null,
            string? makerAddress = null,
            string? conditionId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            string? cursor = null,
            CancellationToken ct = default);
    }
}
