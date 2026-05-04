namespace Polymarket.Net.Enums
{
    /// <summary>
    /// Signature type
    /// </summary>
    public enum SignType
    {
        /// <summary>
        /// Standard EOA (Externally Owned Account) signatures - includes MetaMask, hardware wallets, and any wallet where you control the private key directly
        /// </summary>
        EOA = 0,
        /// <summary>
        /// Email/Magic wallet signatures (delegated signing)
        /// </summary>
        Email = 1,
        /// <summary>
        /// Browser wallet proxy signatures (when using a proxy contract, not direct wallet connections)
        /// </summary>
        Proxy = 2,
        /// <summary>
        /// Deposit-wallet (POLY_1271) signatures: ERC-1271 smart-contract wallet validated via ERC-7739 wrapping. Used by post-migration accounts whose collateral lives in a deposit wallet rather than the owner EOA. Both maker and signer of CLOB orders are set to the deposit wallet address; the inner signature is produced by the owner/session signer.
        /// </summary>
        Poly1271 = 3
    }
}
