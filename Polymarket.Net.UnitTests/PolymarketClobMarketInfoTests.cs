using System.Text.Json;
using NUnit.Framework;
using Polymarket.Net.Objects.Models;

namespace Polymarket.Net.UnitTests
{
    [TestFixture]
    public class PolymarketClobMarketInfoTests
    {
        [TestCase("\"0x0000000000000000000000000000000000000000\"", "0x0000000000000000000000000000000000000000")]
        [TestCase("{\"ctf_exchange\":\"0x0000000000000000000000000000000000000000\"}", "{\"ctf_exchange\":\"0x0000000000000000000000000000000000000000\"}")]
        [TestCase("[\"0x0000000000000000000000000000000000000000\"]", "[\"0x0000000000000000000000000000000000000000\"]")]
        public void Deserialize_ShouldAcceptFlexibleFeeRecipientShape(string recipientJson, string expected)
        {
            // Arrange
            var json = $$"""
            {
              "t": [],
              "mos": "5",
              "mts": "0.001",
              "fd": {
                "r": "0.25",
                "e": "1",
                "to": {{recipientJson}}
              },
              "nr": false,
              "mbf": "0",
              "tbf": "0.25"
            }
            """;

            // Act
            var marketInfo = JsonSerializer.Deserialize<PolymarketClobMarketInfo>(json);

            // Assert
            Assert.That(marketInfo, Is.Not.Null);
            Assert.That(marketInfo!.FeeDetails, Is.Not.Null);
            Assert.That(marketInfo.FeeDetails!.Rate, Is.EqualTo(0.25m));
            Assert.That(marketInfo.FeeDetails.To, Is.EqualTo(expected));
        }
    }
}
