using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.AppCompat.App;
using static Android.Service.Notification.NotificationListenerService;
using static Android.Service.Voice.VoiceInteractionSession;

namespace Wallet.Mobile
{
    [Activity(Theme = "@style/AppTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        protected override void OnResume()
        {
            base.OnResume();

            Task startupWork = new Task(() => { LoadCampaigns(); });
            startupWork.Start();
        }

        private void LoadCampaigns()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://wallet-apis.azurewebsites.net/api/");

                var responseTask = client.GetAsync("campaign");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {

                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    var campaignData = readTask.Result;

                    var intent = new Intent(this, typeof(MainActivity));
                    intent.PutExtra("CampaignData", campaignData);
                    StartActivity(intent);
                }
            }
        }
    }
}

