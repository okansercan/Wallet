// See https://aka.ms/new-console-template for more information
using Wallet.Batch;

Console.WriteLine("Campaign Loader started");
var loader = new CampaignLoader();
loader.Load();
Console.WriteLine("Campaign Loader ended");


