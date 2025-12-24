using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using Polymarket.Net.Clients;
using Polymarket.Net.Objects;
using CryptoExchange.Net.Objects;

namespace Polymarket.Net.UnitTests
{
    [TestFixture()]
    public class PolymarketRestClientTests
    {
        [Test]
        public void CheckSignatureExample1()
        {
            var chainId = 80002;
            var privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var address = "0xf39fd6e51aad88f6f4ce6ab8827279cfffb92266";


            var authProvider = new PolymarketAuthenticationProvider(new PolymarketCredentials(address, privateKey));

            var parameters = new ParameterCollection
            {
                { "salt", "479249096354" },
                { "maker", address },
                { "signer", address },
                { "taker", "0x0000000000000000000000000000000000000000" },
                { "tokenId", "1234" },
                { "makerAmount", "100000000" },
                { "takerAmount", "50000000" },
                { "expiration", "0" },
                { "nonce", "0" },
                { "feeRateBps", "100" },
                { "side", 0 },
                { "signatureType", 0 },
            };

#warning should make the signature match this test: https://github.com/Polymarket/clob-order-utils/blob/75f412893deb05dc8a9161464943e486a757a032/tests/src/exchange.order.builder.test.ts#L273
            var result = authProvider.GetOrderSignature(parameters);
        }

        [Test]
        public void CheckInterfaces()
        {
            CryptoExchange.Net.Testing.TestHelpers.CheckForMissingRestInterfaces<PolymarketRestClient>();
            CryptoExchange.Net.Testing.TestHelpers.CheckForMissingSocketInterfaces<PolymarketSocketClient>();
        }
    }
}
