using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Threading.Tasks;
using Polymarket.Net.Clients;
using Polymarket.Net.Objects.Models;
using Polymarket.Net.Objects.Options;
using Polymarket.Net.Objects;

namespace Polymarket.Net.UnitTests
{
    [TestFixture]
    public class SocketSubscriptionTests
    {
        [Test]
        public async Task ValidateSpotExchangeDataSubscriptions()
        {
            var client = new PolymarketSocketClient(opts =>
            {
                opts.ApiCredentials = new PolymarketCredentials("123", "456");
            });
            var tester = new SocketSubscriptionValidator<PolymarketSocketClient>(client, "Subscriptions/Spot", "XXX");
            //await tester.ValidateAsync<PolymarketModel>((client, handler) => client.SpotApi.SubscribeToXXXUpdatesAsync(handler), "XXX");
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task ValidateConcurrentSpotSubscriptions(bool newDeserialization)
        {
            var logger = new LoggerFactory();
            logger.AddProvider(new TraceLoggerProvider());

            var client = new PolymarketSocketClient(Options.Create(new PolymarketSocketOptions
            {
                ApiCredentials = new PolymarketCredentials("123", "456"),
                OutputOriginalData = true,
                UseUpdatedDeserialization = newDeserialization
            }), logger);

            var tester = new SocketSubscriptionValidator<PolymarketSocketClient>(client, "Subscriptions/Spot", "XXX", "data");
            //await tester.ValidateConcurrentAsync<PolymarketModel>(
            //    (client, handler) => client.SpotApi.ExchangeData.SubscribeToKlineUpdatesAsync("BTCUSDT", Enums.KlineInterval.EightHour, handler),
            //    (client, handler) => client.SpotApi.ExchangeData.SubscribeToKlineUpdatesAsync("BTCUSDT", Enums.KlineInterval.OneHour, handler),
            //    "Concurrent");
        }
    }
}
