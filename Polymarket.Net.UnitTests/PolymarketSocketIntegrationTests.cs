using CryptoExchange.Net.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Polymarket.Net.Clients;
using Polymarket.Net.Objects.Options;

namespace Polymarket.Net.UnitTests
{
    [NonParallelizable]
    internal class PolymarketSocketIntegrationTests : SocketIntegrationTest<PolymarketSocketClient>
    {
        public override bool Run { get; set; } = false;

        public PolymarketSocketIntegrationTests()
        {
        }

        public override PolymarketSocketClient GetClient(ILoggerFactory loggerFactory, bool newDeserialization)
        {
            var key = Environment.GetEnvironmentVariable("APIKEY");
            var sec = Environment.GetEnvironmentVariable("APISECRET");

            Authenticated = key != null && sec != null;
            return new PolymarketSocketClient(Options.Create(new PolymarketSocketOptions
            {
                OutputOriginalData = true,
                UseUpdatedDeserialization = newDeserialization,
                ApiCredentials = Authenticated ? new CryptoExchange.Net.Authentication.ApiCredentials(key, sec) : null
            }), loggerFactory);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task TestSubscriptions(bool newDeserialization)
        {
            //await RunAndCheckUpdate<>(newDeserialization , (client, updateHandler) => client.SpotApi.Account.SubscribeToUserDataUpdatesAsync(default, default, default, default, default, default, default, default), false, true);
            //await RunAndCheckUpdate<>(newDeserialization, (client, updateHandler) => client.SpotApi.ExchangeData.SubscribeToTickerUpdatesAsync("ETHUSDT", updateHandler, default), true, false);
        } 
    }
}
