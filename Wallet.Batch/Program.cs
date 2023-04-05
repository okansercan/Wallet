using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Wallet.Batch;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Console.WriteLine("Campaign Loader started");
var loader = new CampaignLoader(config["ConnectionStrings:DefaultConnection"]);
loader.Load();
Console.WriteLine("Campaign Loader ended");


