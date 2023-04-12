using System;
using System.Text.Json;
using Wallet.MAUI.ViewModel;

namespace Wallet.MAUI.Services
{
	public class CampaignService : ICampaignService
    {
        HttpClient _client;
        JsonSerializerOptions _serializerOptions;
        public List<Campaign> Items { get; private set; }

        public CampaignService()
        {
            _client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<List<Campaign>> GetCampaignsAsync()
        {
            Items = new List<Campaign>();

            Uri uri = new Uri(string.Format(Constants.RestUrl, string.Empty));
            try
            {
                HttpResponseMessage response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Items = JsonSerializer.Deserialize<List<Campaign>>(content, _serializerOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"\tERROR {0}", ex.Message);
            }

            return Items;
        }
    }
}

