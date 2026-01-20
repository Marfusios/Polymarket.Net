using CryptoExchange.Net.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Polymarket.Net.Clients;
using Polymarket.Net.Objects.Options;
using Polymarket.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using System.Collections.Generic;

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

            var result = await CreateClient().ClobApi.ExchangeData.GetMarketAsync("123", default);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Error.ErrorType, Is.EqualTo(ErrorType.UnknownSymbol));
        }

        [Test]
        public async Task TestClobExchangeData()
        {
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetServerTimeAsync(default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetGeographicRestrictionsAsync(default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetSamplingSimplifiedMarketsAsync(default, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetSamplingMarketsAsync(default, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetSimplifiedMarketsAsync(default, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetMarketsAsync(default, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetMarketAsync("0x5eed579ff6763914d78a966c83473ba2485ac8910d0a0914eef6d9fcb33085de", default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetPriceAsync("11862165566757345985240476164489718219056735011698825377388402888080786399275", Enums.OrderSide.Buy, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetPricesAsync(new Dictionary<string, Enums.OrderSide> { { "11862165566757345985240476164489718219056735011698825377388402888080786399275", Enums.OrderSide.Buy } }, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetMidpointPriceAsync("11862165566757345985240476164489718219056735011698825377388402888080786399275", default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetMidpointPricesAsync(new[] { "11862165566757345985240476164489718219056735011698825377388402888080786399275" }, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetPriceHistoryAsync("0x5eed579ff6763914d78a966c83473ba2485ac8910d0a0914eef6d9fcb33085de", default, default, Enums.DataInterval.OneDay, default, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetBidAskSpreadsAsync("11862165566757345985240476164489718219056735011698825377388402888080786399275", default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetBidAskSpreadsAsync(new string[] { "11862165566757345985240476164489718219056735011698825377388402888080786399275" }, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetOrderBookAsync("11862165566757345985240476164489718219056735011698825377388402888080786399275", default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetOrderBooksAsync(new[] { "11862165566757345985240476164489718219056735011698825377388402888080786399275" }, default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetTickSizeAsync("11862165566757345985240476164489718219056735011698825377388402888080786399275", default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetNegativeRiskAsyncAsync("11862165566757345985240476164489718219056735011698825377388402888080786399275", default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetFeeRateBpsAsync("11862165566757345985240476164489718219056735011698825377388402888080786399275", default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetLastTradePriceAsync("11862165566757345985240476164489718219056735011698825377388402888080786399275", default), false);
            await RunAndCheckResult(client => client.ClobApi.ExchangeData.GetLastTradePricesAsync(new[] { "11862165566757345985240476164489718219056735011698825377388402888080786399275" }, default), false);
        }
    }
}
