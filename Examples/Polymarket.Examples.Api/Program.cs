using Polymarket.Net.Interfaces.Clients;
using Microsoft.AspNetCore.Mvc;
using Polymarket.Net.Enums;
using Polymarket.Net.Objects;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the Polymarket services
builder.Services.AddPolymarket();

// OR to provide API credentials for accessing private endpoints, or setting other options:
/*
builder.Services.AddPolymarket(options =>
{
    options.ApiCredentials = new PolymarketCredentials(
    SignType.EOA, // Externally Owned Account wallet, when using an existing wallet to connect to polymarket
    "0x00..", // The private key for the wallet
    "KEY", // The L2 API key as previously retrieved with `polymarketRestClient.ClobApi.Account.GetOrCreateApiCredentialsAsync()`
    "SEC", // The L2 API secret as previously retrieved with `polymarketRestClient.ClobApi.Account.GetOrCreateApiCredentialsAsync()`
    "PASS" // The L2 API passphrase as previously retrieved with `polymarketRestClient.ClobApi.Account.GetOrCreateApiCredentialsAsync()`
        );
    options.Rest.RequestTimeout = TimeSpan.FromSeconds(5);
});
*/

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Map the endpoint and inject the rest client
app.MapGet("/{TokenId}", async ([FromServices] IPolymarketRestClient client, string tokenId) =>
{
    var result = await client.ClobApi.ExchangeData.GetLastTradePriceAsync(tokenId);
    return result.Data?.LastTradePrice;
})
.WithOpenApi();


app.MapGet("/Balances", async ([FromServices] IPolymarketRestClient client) =>
{
    var result = await client.ClobApi.Account.GetBalanceAllowanceAsync(Polymarket.Net.Enums.AssetType.Collateral);
    return (object)(result.Success ? result.Data : result.Error!);
})
.WithOpenApi();

app.Run();