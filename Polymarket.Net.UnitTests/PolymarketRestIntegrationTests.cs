using CryptoExchange.Net.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Polymarket.Net.Clients;
using Polymarket.Net.Objects.Options;
using Polymarket.Net.Objects;

namespace Polymarket.Net.UnitTests
{
    [NonParallelizable]
    public class PolymarketRestIntegrationTests : RestIntegrationTest<PolymarketRestClient>
    {
        public override bool Run { get; set; } = false;

        public override PolymarketRestClient GetClient(ILoggerFactory loggerFactory)
        {
            var key = Environment.GetEnvironmentVariable("APIKEY");
            var sec = Environment.GetEnvironmentVariable("APISECRET");

            Authenticated = key != null && sec != null;
            return new PolymarketRestClient(null, loggerFactory, Options.Create(new PolymarketRestOptions
            {
                AutoTimestamp = false,
                OutputOriginalData = true,
                ApiCredentials = Authenticated ? new PolymarketCredentials(key, sec) : null
            }));
        }

        [Test]
        public async Task TestErrorResponseParsing()
        {
            if (!ShouldRun())
                return;

#warning Implement error response
            //var result = await CreateClient().SpotApi.ExchangeData.GetTickerAsync("TSTTST", default);

            //Assert.That(result.Success, Is.False);
            //Assert.That(result.Error.Code, Is.EqualTo(-1121));
        }

        [Test]
        public async Task TestSpotExchangeData()
        {
            //await RunAndCheckResult(client => client.SpotApi.ExchangeData.PingAsync(CancellationToken.None), false);
        }
    }
}
