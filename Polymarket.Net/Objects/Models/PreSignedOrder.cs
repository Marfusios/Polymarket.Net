using CryptoExchange.Net.Objects;
using Polymarket.Net;
using Polymarket.Net.Enums;
using System;
using System.Text;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Result of <see cref="Interfaces.Clients.ClobApi.IPolymarketRestClientClobApiTrading.BuildAndSignOrder"/>.
    /// An assembled and EIP-712 signed order body ready to submit via PlaceSignedOrderAsync.
    /// Each instance encodes a unique salt; do not reuse after a successful or failed submission.
    /// </summary>
    public sealed class PreSignedOrder
    {
        private string? _defaultSubmitBody;
        private byte[]? _defaultSubmitBodyUtf8;

        /// <summary>The signed parameter envelope (passed to /order endpoint).</summary>
        internal ParameterCollection OrderParameters { get; }

        /// <summary>Token id (UP or DOWN).</summary>
        public string TokenId { get; }

        /// <summary>Buy or Sell.</summary>
        public OrderSide Side { get; }

        /// <summary>Limit price (0..1).</summary>
        public decimal Price { get; }

        /// <summary>Quantity (shares).</summary>
        public decimal Quantity { get; }

        /// <summary>Maker amount (USDC for buys, shares for sells) as it was signed.</summary>
        public decimal MakerAmount { get; }

        /// <summary>Taker amount (shares for buys, USDC for sells) as it was signed.</summary>
        public decimal TakerAmount { get; }

        /// <summary>Salt that acts as the server-side replay nonce.</summary>
        public ulong Salt { get; }

        /// <summary>Wall clock at signing.</summary>
        public DateTimeOffset SignedAt { get; }

        /// <summary>Whether the underlying market is a negative-risk (negRisk) market — affects
        /// the verifying contract address that was baked into the signature.</summary>
        public bool NegativeRisk { get; }

        /// <summary>
        /// True for "boot" entries pre-signed at the maximum buy-price ceiling so the first
        /// signal in a new market can be caught regardless of where the live limit lands.
        /// Wide-limit entries skip the cache's price-drift upper bound (the cached price will
        /// always be much higher than the live signal price). Polymarket fills at the prevailing
        /// best ask up to the signed makerAmount, so the actual fill price is unaffected — only
        /// the worst-case capital commitment is widened.
        /// </summary>
        public bool IsWideLimit { get; }

        /// <summary>
        /// Pre-build the JSON body that will be sent to the CLOB /order endpoint.
        /// L2 HMAC headers are intentionally not cached because they include the send timestamp.
        /// </summary>
        public void PrepareSubmitBody(
            string owner,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? deferExecution = null)
        {
            _ = GetSubmitBody(owner, timeInForce, postOnly, deferExecution);
            _ = GetSubmitBodyUtf8(owner, timeInForce, postOnly, deferExecution);
        }

        internal PreSignedOrder(
            ParameterCollection orderParameters,
            string tokenId,
            OrderSide side,
            decimal price,
            decimal quantity,
            decimal makerAmount,
            decimal takerAmount,
            ulong salt,
            DateTimeOffset signedAt,
            bool negativeRisk,
            bool isWideLimit = false)
        {
            OrderParameters = orderParameters;
            TokenId = tokenId;
            Side = side;
            Price = price;
            Quantity = quantity;
            MakerAmount = makerAmount;
            TakerAmount = takerAmount;
            Salt = salt;
            SignedAt = signedAt;
            NegativeRisk = negativeRisk;
            IsWideLimit = isWideLimit;
        }

        internal string GetSubmitBody(
            string owner,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? deferExecution = null)
        {
            var effectiveTimeInForce = timeInForce ?? TimeInForce.GoodTillCanceled;
            var effectivePostOnly = postOnly ?? false;
            var effectiveDeferExecution = deferExecution ?? false;
            if (effectiveTimeInForce == TimeInForce.GoodTillCanceled &&
                !effectivePostOnly &&
                !effectiveDeferExecution &&
                _defaultSubmitBody != null)
            {
                return _defaultSubmitBody;
            }

            var parameters = new ParameterCollection();
            parameters.Add("order", OrderParameters);
            parameters.Add("owner", owner);
            parameters.AddEnum("orderType", effectiveTimeInForce);
            parameters.Add("postOnly", effectivePostOnly);
            parameters.Add("deferExec", effectiveDeferExecution);

            var body = PolymarketAuthenticationProvider.SerializeBody(parameters);
            if (effectiveTimeInForce == TimeInForce.GoodTillCanceled &&
                !effectivePostOnly &&
                !effectiveDeferExecution)
            {
                _defaultSubmitBody = body;
            }

            return body;
        }

        /// <summary>
        /// UTF-8 encoding of <see cref="GetSubmitBody"/>, cached for the default-flags
        /// case so the submit path can use a ByteArrayContent without re-encoding the
        /// body on every order.
        /// </summary>
        internal byte[] GetSubmitBodyUtf8(
            string owner,
            TimeInForce? timeInForce = null,
            bool? postOnly = null,
            bool? deferExecution = null)
        {
            var effectiveTimeInForce = timeInForce ?? TimeInForce.GoodTillCanceled;
            var effectivePostOnly = postOnly ?? false;
            var effectiveDeferExecution = deferExecution ?? false;
            var isDefault = effectiveTimeInForce == TimeInForce.GoodTillCanceled &&
                            !effectivePostOnly &&
                            !effectiveDeferExecution;
            if (isDefault && _defaultSubmitBodyUtf8 != null)
                return _defaultSubmitBodyUtf8;

            var bytes = Encoding.UTF8.GetBytes(GetSubmitBody(owner, timeInForce, postOnly, deferExecution));
            if (isDefault)
                _defaultSubmitBodyUtf8 = bytes;

            return bytes;
        }
    }
}
