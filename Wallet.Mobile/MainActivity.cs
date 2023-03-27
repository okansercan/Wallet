using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Azure;
using Azure.Core;
using Azure.AI.Language.Conversations;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json;
using Wallet.Mobile.Adapters;
using Wallet.Mobile.Models;
using System.Text.Json;

namespace Wallet.Mobile
{
    [Activity(Label = "@string/app_name")]
    public class MainActivity : AppCompatActivity
    {
        List<Campaign> campaignData;
        Button _button;
        Spinner spnBrand, spnSector;
        ListView listCampaign;
        String brand, sector;
        SpeechConfig speechConfig;
        ConversationAnalysisClient lsClient;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_main);

            SetSpeechConfig();
            SetLanguageConfig();

            campaignData = JsonConvert.DeserializeObject<List<Campaign>>(Intent.GetStringExtra("CampaignData"));

            spnBrand = FindViewById<Spinner>(Resource.Id.spnBrand);
            spnBrand.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spnBrand_itemSelected);

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, GetBrands());
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spnBrand.Adapter = adapter;

            spnSector = FindViewById<Spinner>(Resource.Id.spnSector);
            spnSector.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spnSector_itemSelected);

            ImageButton btnSpeak = FindViewById<ImageButton>(Resource.Id.btnSpeak);
            btnSpeak.Click += async (sender, args) =>
            {
                await ProcessSpeech();
            };
        }

        private void spnBrand_itemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            brand = spnBrand.GetItemAtPosition(e.Position).ToString();

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, GetSectors(brand));
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spnSector.Adapter = adapter;
        }

        private void spnSector_itemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            sector = spnSector.GetItemAtPosition(e.Position).ToString();
            var campaigns = campaignData.Where(c => c.Brand == brand && c.Sector == sector).ToList();
            SetCampaignList(campaigns);
        }

        private void SetCampaignList(List<Campaign> campaigns)
        {
            listCampaign = FindViewById<ListView>(Resource.Id.listCampaign);
            var adapter = new CampaignAdapter(this, campaigns);
            listCampaign.Adapter = adapter;
        }

        private string[] GetBrands()
        {
            return campaignData.Select(c => c.Brand).Distinct().ToArray();
        }

        private string[] GetSectors(string brand)
        {
            return campaignData.Where(c => c.Brand == brand).Select(c => c.Sector).Distinct().ToArray();
        }

        private void SetSpeechConfig()
        {
            string speechPredictionKey = "06f95467fd874ed89d1b54bcb76dd13e";
            string speechRegion = "westeurope";

            // Configure speech service and get intent recognizer
            speechConfig = SpeechConfig.FromSubscription(speechPredictionKey, speechRegion);
            speechConfig.SpeechRecognitionLanguage = "tr-TR";
        }

        private void SetLanguageConfig()
        {
            string predictionEndpoint = "https://okan-lang.cognitiveservices.azure.com/";
            string predictionKey = "fab776a596284e2c8bae42ec0bdef161";

            Uri lsEndpoint = new Uri(predictionEndpoint);
            AzureKeyCredential lsCredential = new AzureKeyCredential(predictionKey);

            lsClient = new ConversationAnalysisClient(lsEndpoint, lsCredential);
        }

        async Task ProcessSpeech()
        {
            // Configure speech recognition
            using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            String userText;

            // Process speech input
            SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
            if (speech.Reason == ResultReason.RecognizedSpeech)
            {
                userText = speech.Text;
                ProcessSpeech(userText);
            }
            else
            {
                if (speech.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(speech);
                    Console.WriteLine(cancellation.ErrorDetails);
                }
            }
        }

        private void ProcessSpeech(string userText)
        {
            // Call the Language service model to get intent and entities
            var projectName = "Wallet";
            var deploymentName = "production";
            string brand = string.Empty, campaign = string.Empty;

            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = userText,
                        id = "1",
                        participantId = "1",
                    }
                },
                parameters = new
                {
                    projectName,
                    deploymentName,

                    // Use Utf16CodeUnit for strings in .NET.
                    stringIndexType = "Utf16CodeUnit",
                },
                kind = "Conversation",
            };

            try
            {
                Response response = lsClient.AnalyzeConversation(RequestContent.Create(data));

                using JsonDocument result = JsonDocument.Parse(response.ContentStream);
                JsonElement conversationalTaskResult = result.RootElement;
                JsonElement conversationPrediction = conversationalTaskResult.GetProperty("result").GetProperty("prediction");

                var topIntent = conversationPrediction.GetProperty("topIntent").GetString();

                // Apply the appropriate action
                switch (topIntent)
                {
                    case "FindCampaign":
                        foreach (JsonElement entity in conversationPrediction.GetProperty("entities").EnumerateArray())
                        {
                            if (entity.GetProperty("category").GetString() == "brand")
                            {
                                brand = entity.GetProperty("text").GetString();
                            }
                            else
                            {
                                campaign = entity.GetProperty("text").GetString();
                            }
                        }
                        break;
                    default:
                        break;
                }

                var campaigns = FindCampaigns(brand, campaign);
                SetCampaignList(campaigns);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private List<Campaign> FindCampaigns(string brand, string campaign)
        {
            List<Campaign> campaigns = new List<Campaign>();

            if (!string.IsNullOrEmpty(brand))
            {
                campaigns = campaignData.Where(c => c.Brand.ToUpper().Equals(brand.ToUpper())).ToList();
            }

            if (!string.IsNullOrEmpty(campaign))
            {
                if (campaigns.Count > 0)
                {
                    campaigns = campaigns.Where(c => c.Sector.ToUpper().Contains(campaign.ToUpper())).ToList();
                }
                else
                {
                    campaigns = campaignData.Where(c => c.Sector.ToUpper().Equals(campaign.ToUpper())).ToList();
                }
            }

            return campaigns;
        }
    }
}

