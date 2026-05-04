using CryptoExchange.Net.Objects;
using Polymarket.Net.Enums;
using System;

namespace Polymarket.Net.Objects.Models
{
    /// <summary>
    /// Result of <see cref="Interfaces.Clients.ClobApi.IPolymarketRestClientClobApiTrading.BuildAndSignOrder"/>.
    /// An assembled and EIP-712 signed order body ready to submit via PlaceSignedOrderAsync.
    /// Each instance encodes a unique salt; do not reuse after a successful or failed submission.
    /// </summary>
    public sealed class PreSignedOrder
    {
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
    }
}
