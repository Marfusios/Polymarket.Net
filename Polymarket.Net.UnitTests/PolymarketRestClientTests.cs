using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using Polymarket.Net.Clients;
using Polymarket.Net.Objects;
using CryptoExchange.Net.Objects;
using System;

namespace Polymarket.Net.UnitTests
{
    [TestFixture()]
    public class PolymarketRestClientTests
    {
        [Test]
        public void CheckSignatureExample1()
        {
            uint chainId = 80002;
            var privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var address = "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266";


            var authProvider = new PolymarketAuthenticationProvider(new PolymarketCredentials(Enums.SignType.EOA, privateKey, address));

            var parameters = new ParameterCollection
            {
                { "salt", "479249096354" },
                { "maker", address },
                { "signer", address },
                { "tokenId", "1234" },
                { "makerAmount", "100000000" },
                { "takerAmount", "50000000" },
                { "side", "BUY" },
                { "signatureType", 0 },
                { "timestamp", "1710000000000" },
                { "metadata", "0x0000000000000000000000000000000000000000000000000000000000000000" },
                { "builder", "0x0000000000000000000000000000000000000000000000000000000000000000" },
            };

            var result = authProvider.GetOrderSignature(parameters, chainId, false).ToLower();
            Assert.That(result, Is.EqualTo("0xbc858bfc90853f8e417f8c4b23e31d9d97e1e5c211f263b855093748ce599ef26007c93c60a9b5dbe6ce3ef250cf4bbc10e64d9c801cdfc8c7e13b7c3ba493d41c"));
        }

        [Test]
        public void CheckSignatureExample2()
        {
            uint chainId = 137;
            var privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var address = "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266";

            var authProvider = new PolymarketAuthenticationProvider(new PolymarketCredentials(Enums.SignType.EOA, privateKey, address));

            var parameters = new ParameterCollection
            {
                { "salt", "1515433236867" },
                { "maker", address },
                { "signer", address },
                { "tokenId", "11862165566757345985240476164489718219056735011698825377388402888080786399275" },
                { "makerAmount", "5000" },
                { "takerAmount", "5000000" },
                { "side", "BUY" },
                { "signatureType", 1 },
                { "timestamp", "1710000000000" },
                { "metadata", "0x0000000000000000000000000000000000000000000000000000000000000000" },
                { "builder", "0x0000000000000000000000000000000000000000000000000000000000000000" },
            };

            var result = authProvider.GetOrderSignature(parameters, chainId, true).ToLower();
            Assert.That(result, Is.EqualTo("0xfc3b6facc2b9c36f11c4224247336f704d9476978fafa667fb051bcf68ad9402372f6025de06b8ca3efb87267658d44e7466b18e2563ee7ec5110abf8f43e8031b"));
        }

        [Test]
        public void CheckInterfaces()
        {
            CryptoExchange.Net.Testing.TestHelpers.CheckForMissingRestInterfaces<PolymarketRestClient>();
            CryptoExchange.Net.Testing.TestHelpers.CheckForMissingSocketInterfaces<PolymarketSocketClient>();
        }

        [Test]
        public void NormalizeOrderPrice_ShouldRoundToThreeDecimals()
        {
            var normalized = Clients.ClobApi.PolymarketRestClientClobApiTrading.NormalizeOrderPrice(0.539004975124378m);
            Assert.That(normalized, Is.EqualTo(0.539m));
        }

        [Test]
        public void ConvertToClobBaseUnits_ShouldProduceIntegerMicroUnits()
        {
            var normalizedPrice = Clients.ClobApi.PolymarketRestClientClobApiTrading.NormalizeOrderPrice(0.539004975124378m);
            var makerAmount = Clients.ClobApi.PolymarketRestClientClobApiTrading.ConvertToClobBaseUnits(5m * normalizedPrice);
            Assert.That(makerAmount, Is.EqualTo(2695000m));
        }

        [Test]
        public void NormalizeBytes32_ShouldDefaultEmptyValuesToZeroBytes32()
        {
            var normalized = Clients.ClobApi.PolymarketRestClientClobApiTrading.NormalizeBytes32(null, "metadata");

            Assert.That(normalized, Is.EqualTo("0x0000000000000000000000000000000000000000000000000000000000000000"));
        }

        [Test]
        public void NormalizeBytes32_ShouldLowercaseValidValues()
        {
            var normalized = Clients.ClobApi.PolymarketRestClientClobApiTrading.NormalizeBytes32(
                "0xABCDEFabcdef0000000000000000000000000000000000000000000000000000",
                "builderCode");

            Assert.That(normalized, Is.EqualTo("0xabcdefabcdef0000000000000000000000000000000000000000000000000000"));
        }

        [Test]
        public void NormalizeBytes32_ShouldRejectInvalidValues()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                Clients.ClobApi.PolymarketRestClientClobApiTrading.NormalizeBytes32("builder-1", "builderCode"));

            Assert.That(exception!.ParamName, Is.EqualTo("builderCode"));
        }
    }
}
