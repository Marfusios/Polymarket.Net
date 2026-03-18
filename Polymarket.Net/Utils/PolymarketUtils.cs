using CryptoExchange.Net.Objects;
using Polymarket.Net.Interfaces.Clients.ClobApi;
using Polymarket.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Polymarket.Net.Utils
{
    /// <summary>
    /// Util methods for the Polymarket API
    /// </summary>
    public static class PolymarketUtils
    {
        private static Dictionary<string, Dictionary<string, PolymarketOrderBook>> _tokenInfos = new Dictionary<string, Dictionary<string, PolymarketOrderBook>>();

        private static readonly SemaphoreSlim _semaphoreSpot = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Update the internal spot symbol info
        /// </summary>
        public static async Task<CallResult<PolymarketOrderBook>> GetTokenInfoAsync(
            string tokenId,
            IPolymarketRestClientClobApi client,
            CancellationToken ct = default)
        {
            var envName = client.ClientOptions.Environment.Name;

            await _semaphoreSpot.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (envName.Equals("UnitTest", StringComparison.Ordinal))
                    return new CallResult<PolymarketOrderBook>(new PolymarketOrderBook { });

                // Re-check after acquiring semaphore (another thread may have populated it)
                if (_tokenInfos.TryGetValue(envName, out var envTokens) && envTokens.TryGetValue(tokenId, out var cachedTokenInfo))
                    return new CallResult<PolymarketOrderBook>(cachedTokenInfo);

                var tokenInfo = await client.ExchangeData.GetOrderBookAsync(tokenId, ct).ConfigureAwait(false);
                if (!tokenInfo)
                    return tokenInfo;

                if (!_tokenInfos.ContainsKey(envName))
                    _tokenInfos[envName] = new Dictionary<string, PolymarketOrderBook>();

                _tokenInfos[envName][tokenId] = tokenInfo.Data;
                return tokenInfo;
            }
            finally
            {
                _semaphoreSpot.Release();
            }
        }
    }
}
