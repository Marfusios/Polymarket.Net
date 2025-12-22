using CryptoExchange.Net.Objects;
using Polymarket.Net.Interfaces.Clients.ClobApi;
using Polymarket.Net.Objects.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Polymarket.Net.Clients.ClobApi
{
    /// <inheritdoc />
    internal class PolymarketRestClientClobApiAccount : IPolymarketRestClientClobApiAccount
    {
        private static readonly RequestDefinitionCache _definitions = new RequestDefinitionCache();
        private readonly PolymarketRestClientClobApi _baseClient;

        internal PolymarketRestClientClobApiAccount(PolymarketRestClientClobApi baseClient)
        {
            _baseClient = baseClient;
        }

        public async Task<WebCallResult<PolymarketCreds>> CreateApiCredentialsAsync(long? nonce = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("nonce", nonce);
            var request = _definitions.GetOrCreate(HttpMethod.Post, "auth/api-key", PolymarketExchange.RateLimiter.Polymarket, 1, true);
            var result = await _baseClient.SendAsync<PolymarketCreds>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCreds>> GetApiCredentialsAsync(long? nonce = null, CancellationToken ct = default)
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("nonce", nonce);
            var request = _definitions.GetOrCreate(HttpMethod.Get, "auth/derive-api-key", PolymarketExchange.RateLimiter.Polymarket, 1, true);
            var result = await _baseClient.SendAsync<PolymarketCreds>(request, parameters, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<WebCallResult<PolymarketCreds>> GetOrCreateApiCredentialsAsync(long? nonce = null)
        {
            var getResult = await GetApiCredentialsAsync(nonce).ConfigureAwait(false);
            if (getResult)
                return getResult;

            return await CreateApiCredentialsAsync(nonce).ConfigureAwait(false);
        }
    }
}
