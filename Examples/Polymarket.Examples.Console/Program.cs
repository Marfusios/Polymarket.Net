
using Polymarket.Net.Clients;

// REST
var restClient = new PolymarketRestClient();
var tokenId = "4153292802911610701832309484716814274802943278345248636922528170020319407796";
var ticker = await restClient.ClobApi.ExchangeData.GetLastTradePriceAsync(tokenId);
Console.WriteLine($"Rest client last price: {ticker.Data.LastTradePrice}");

Console.WriteLine();
Console.WriteLine("Press enter to start websocket subscription");
Console.ReadLine();

// Websocket
var socketClient = new PolymarketSocketClient();
var subscription = await socketClient.ClobApi.SubscribeToTokenUpdatesAsync([tokenId], onLastTradePriceUpdate: x =>
{
    Console.WriteLine(x.Data.Price);
});

Console.ReadLine();
