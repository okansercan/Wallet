using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Util;
using Android.Widget;
using Newtonsoft.Json;
using System.Collections.Generic;
using Wallet.Mobile.Models;
using System.Linq;
using System.Reflection.Emit;
using Wallet.Mobile.Adapters;

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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_main);

            campaignData = JsonConvert.DeserializeObject<List<Campaign>>(Intent.GetStringExtra("CampaignData"));

            spnBrand = FindViewById<Spinner>(Resource.Id.spnBrand);
            spnBrand.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spnBrand_itemSelected);

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, GetBrands());
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spnBrand.Adapter = adapter;

            spnSector = FindViewById<Spinner>(Resource.Id.spnSector);
            spnSector.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spnSector_itemSelected);

            /*_button = FindViewById<Button>(Resource.Id.MyButton);
            _button.Click += (sender, args) =>
            {
                string message = string.Format("Toplam Kampanya adedi: {0}", campaignData.Count);
                _button.Text = message;
            };*/
        }

        private string[] GetBrands()
        {
            return campaignData.Select(c => c.Brand).Distinct().ToArray();
        }

        private string[] GetSectors(string brand)
        {
            return campaignData.Where(c => c.Brand == brand).Select(c => c.Sector).Distinct().ToArray();
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
    }
}

