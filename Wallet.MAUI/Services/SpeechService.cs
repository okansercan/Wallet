
using Azure;
using Azure.Core;
using Azure.AI.Language.Conversations;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Text.Json;

namespace Wallet.MAUI.Services
{
	public class SpeechService : ISpeechService
	{
        SpeechConfig speechConfig;
        ConversationAnalysisClient lsClient;

        public SpeechService()
		{
            SetSpeechConfig();
            SetLanguageConfig();
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

        public async Task<string> ProcessSpeech()
        {
            string campaignText = string.Empty;

            // Configure speech recognition
            using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            String userText;

            // Process speech input
            SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
            if (speech.Reason == ResultReason.RecognizedSpeech)
            {
                userText = speech.Text;
                campaignText = AnalyzeSpeech(userText);
            }
            else
            {
                if (speech.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(speech);
                    Console.WriteLine(cancellation.ErrorDetails);
                }
            }

            return campaignText;
        }

        private string AnalyzeSpeech(string userText)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return campaign;
        }
    }
}

