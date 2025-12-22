using CryptoExchange.Net.Objects;
using Polymarket.Net.Objects.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Polymarket.Net.Interfaces.Clients.ClobApi
{
    /// <summary>
    /// Polymarket Clob account endpoints. Account endpoints include balance info, withdraw/deposit info and requesting and account settings
    /// </summary>
    public interface IPolymarketRestClientClobApiAccount
    {
        Task<WebCallResult<PolymarketCreds>> CreateApiCredentialsAsync(long? nonce = null, CancellationToken ct = default);

        Task<WebCallResult<PolymarketCreds>> GetApiCredentialsAsync(long? nonce = null, CancellationToken ct = default);

        Task<WebCallResult<PolymarketCreds>> GetOrCreateApiCredentialsAsync(long? nonce = null);
    }
}
