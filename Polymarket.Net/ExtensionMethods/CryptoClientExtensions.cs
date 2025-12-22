using CryptoExchange.Net.Interfaces.Clients;
using Polymarket.Net.Clients;
using Polymarket.Net.Interfaces.Clients;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Extensions for the ICryptoRestClient and ICryptoSocketClient interfaces
    /// </summary>
    public static class CryptoClientExtensions
    {
        /// <summary>
        /// Get the Polymarket REST Api client
        /// </summary>
        /// <param name="baseClient"></param>
        /// <returns></returns>
        public static IPolymarketRestClient Polymarket(this ICryptoRestClient baseClient) => baseClient.TryGet<IPolymarketRestClient>(() => new PolymarketRestClient());

        /// <summary>
        /// Get the Polymarket Websocket Api client
        /// </summary>
        /// <param name="baseClient"></param>
        /// <returns></returns>
        public static IPolymarketSocketClient Polymarket(this ICryptoSocketClient baseClient) => baseClient.TryGet<IPolymarketSocketClient>(() => new PolymarketSocketClient());
    }
}
