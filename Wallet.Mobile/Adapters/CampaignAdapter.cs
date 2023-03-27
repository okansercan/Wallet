using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Emit;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Wallet.Mobile.Models;
using Xamarin.Essentials;

namespace Wallet.Mobile.Adapters
{
    public class CampaignAdapter : BaseAdapter<Campaign>
    {
        List<Campaign> _liste;
        Context _context;

        public CampaignAdapter(Context context, List<Campaign> liste)
        {
            _liste = liste;
            _context = context;
        }

        public override Campaign this[int position]
        {
            get { return _liste[position]; }
        }

        public override int Count
        {
            get { return _liste.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View satir = convertView;
            if (satir == null)
                satir = LayoutInflater.From(_context).Inflate(Resource.Layout.campaign_row, null, false);

            TextView txtBrand = satir.FindViewById<TextView>(Resource.Id.txtBrand);
            txtBrand.Text = _liste[position].Brand;

            TextView txtTitle = satir.FindViewById<TextView>(Resource.Id.txtTitle);
            txtTitle.Text = _liste[position].Title;

            ImageView imageView = satir.FindViewById<ImageView>(Resource.Id.imageView);
            var imageBitmap = GetImageBitmapFromUrl(_liste[position].ImageUrl);

            if (imageBitmap != null)
                imageView.SetImageBitmap(imageBitmap);

            Button btnDetail = satir.FindViewById<Button>(Resource.Id.btnDetail);

            btnDetail.Click += (sender, args) =>
            {
                Uri uri = new Uri(_liste[position].DetailUrl);
                Browser.OpenAsync(uri, new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Show
                });
            };

            return satir;
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            try
            {
                using (var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(url);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    }
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }

            return imageBitmap;
        }
    }
}

