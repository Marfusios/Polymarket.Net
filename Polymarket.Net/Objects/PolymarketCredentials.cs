using CryptoExchange.Net.Authentication;
using Polymarket.Net.Clients.ClobApi;
using Polymarket.Net.Enums;

namespace Polymarket.Net.Objects
{
    /// <summary>
    /// Polymarket credentials
    /// </summary>
    public class PolymarketCredentials : ApiCredentials
    {
        /// <summary>
        /// Signature type
        /// </summary>
        public SignType SignatureType { get; set; }
        /// <summary>
        /// The polymarket funding address when using email/magic wallets. Can be found in your account in the web interface
        /// </summary>
        public string? PolymarketFundingAddress { get; set; }
        /// <summary>
        /// Private key for the trading wallet
        /// </summary>
        public string L1PrivateKey { get; set; }
        /// <summary>
        /// The layer 2 API key previously obtained with <see cref="PolymarketRestClientClobApiAccount.GetOrCreateApiCredentialsAsync"/>
        /// </summary>
        public string? L2ApiKey { get; set; }
        /// <summary>
        /// The layer 2 secret previously obtained with <see cref="PolymarketRestClientClobApiAccount.GetOrCreateApiCredentialsAsync"/>
        /// </summary>
        public string? L2Secret { get; set; }
        /// <summary>
        /// The layer 2 passphrase previously obtained with <see cref="PolymarketRestClientClobApiAccount.GetOrCreateApiCredentialsAsync"/>
        /// </summary>
        public string? L2Pass { get; set; }

        /// <summary>
        /// Deposit-wallet (POLY_1271) accounts created after 2026-04-28: bind the L2
        /// API key to the DEPOSIT WALLET instead of the owner EOA. L1 auth then sends
        /// POLY_ADDRESS = funding address with an ERC-7739-wrapped ClobAuth signature
        /// (validated through the wallet's ERC-1271 hook), and L2 requests claim the
        /// wallet address. Required for fresh deposit-wallet accounts — the venue
        /// rejects orders whose signer (the wallet) differs from the api-key address.
        /// Leave false for grandfathered accounts whose EOA-bound keys still work.
        /// </summary>
        public bool WalletBoundL1Auth { get; set; }

        /// <summary>
        /// Create new API credentials with a Polymarket public address and the private key for the funding address
        /// </summary>
        /// <param name="signType">The signature type</param>
        /// <param name="polymarketFundingAddress">The polymarket funding address when using email/magic wallets. Can be found in your account in the web interface</param>
        /// <param name="l1PrivateKey">Private key for the trading wallet</param>
        public PolymarketCredentials(SignType signType, string l1PrivateKey, string? polymarketFundingAddress = null) : base(polymarketFundingAddress ?? "-", l1PrivateKey, null, ApiCredentialsType.Hmac)
        {
            SignatureType = signType;
            PolymarketFundingAddress = polymarketFundingAddress;
            L1PrivateKey = l1PrivateKey;
        }

        /// <summary>
        /// Create new API credentials with a Polymarket public address, the private key for the funding address and previously obtained layer 2 credentials using <see cref="PolymarketRestClientClobApiAccount.GetOrCreateApiCredentialsAsync"/>
        /// </summary>
        /// <param name="signType">The signature type</param>
        /// <param name="polymarketFundingAddress">The polymarket funding address when using email/magic wallets. Can be found in your account in the web interface</param>
        /// <param name="l1PrivateKey">Private key for the trading wallet</param>
        /// <param name="l2Key">The layer 2 API key previously obtained with <see cref="PolymarketRestClientClobApiAccount.GetOrCreateApiCredentialsAsync"/></param>
        /// <param name="l2Secret">The layer 2 secret previously obtained with <see cref="PolymarketRestClientClobApiAccount.GetOrCreateApiCredentialsAsync"/></param>
        /// <param name="l2Pass">The layer 2 passphrase previously obtained with <see cref="PolymarketRestClientClobApiAccount.GetOrCreateApiCredentialsAsync"/></param>
        public PolymarketCredentials(
            SignType signType,
            string l1PrivateKey,
            string l2Key, 
            string l2Secret, 
            string l2Pass,
            string? polymarketFundingAddress = null) : base(l2Key, l2Secret, l2Pass, ApiCredentialsType.Hmac)
        {
            SignatureType = signType;
            PolymarketFundingAddress = polymarketFundingAddress;
            L1PrivateKey = l1PrivateKey;
            L2ApiKey = l2Key;
            L2Secret = l2Secret;
            L2Pass = l2Pass;
        }

        /// <inheritdoc/>
        public override ApiCredentials Copy()
        {
            if (L2ApiKey != null)
            {
                return new PolymarketCredentials(SignatureType, L1PrivateKey, L2ApiKey, L2Secret!, L2Pass!, PolymarketFundingAddress)
                {
                    CredentialType = this.CredentialType,
                    WalletBoundL1Auth = this.WalletBoundL1Auth
                };
            }
            else
            {
                return new PolymarketCredentials(SignatureType, L1PrivateKey, PolymarketFundingAddress)
                {
                    CredentialType = this.CredentialType,
                    WalletBoundL1Auth = this.WalletBoundL1Auth
                };
            }
        }
    }
}
