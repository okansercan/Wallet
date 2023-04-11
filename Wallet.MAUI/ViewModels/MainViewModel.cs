using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Android.Widget;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Wallet.MAUI.ViewModel;

public partial class MainViewModel : ObservableObject
{
    readonly IList<Campaign> source;

    public MainViewModel()
    {
        Campaigns = new ObservableCollection<Campaign>();
        source = new List<Campaign>();
        GetCampaignCollection();
    }

    [ObservableProperty]
    ObservableCollection<Campaign> campaigns;

    [ObservableProperty]
    string text;

    [RelayCommand]
    void Filter()
    {
        if (string.IsNullOrWhiteSpace(Text))
            return;

        var filteredItems = source.Where(campaign => campaign.Title.ToLower().Contains(Text.ToLower())).ToList();

        Campaigns.Clear();
        foreach (var campaign in filteredItems)
        {
            Campaigns.Add(campaign);
        }
    }

    [RelayCommand]
    void VoiceFilter()
    {
        return;
    }

    [RelayCommand]
    void Favorite(Campaign campaign)
    {
        return;
    }

    [RelayCommand]
    void Navigate(Campaign campaign)
    {
        return;
    }

    private void GetCampaignCollection()
    {
        source.Add(new Campaign
        {
            Brand = "Bonus",
            LastDate = "30 Nisan",
            Sector = "Eğitim",
            Title = "Tüm eğitim harcamalarınıza ücretsiz +4 taksit fırsatı!",
            ImageUrl = "https://www.bonus.com.tr/assets/images/imported/egit_220722.jpg",
            DetailUrl = "https://www.bonus.com.tr/kampanyalar/egitim-harcama-taksit-kampanyalari"
        });

        source.Add(new Campaign
        {
            Brand = "Bonus",
            Sector = "Turizm",
            Title = "Seyahat harcamanıza 150 tl bonus!",
            ImageUrl = "https://www.bonus.com.tr/assets/images/imported/sey_250522.jpg",
            DetailUrl = "https://www.bonus.com.tr/kampanyalar/seyahat-harcamaniza-bonus-kampanya"
        });

        source.Add(new Campaign
        {
            Brand = "WorldCard",
            Sector = "Akaryakıt & Otogaz",
            Title = "Opet Worldcard ile Araç Yıkama Sektöründe 200 TL ve üzeri harcamalarınızda10 TL, toplamda 30 TL Opet Puan!",
            ImageUrl = "https://www.worldcard.com.tr/medium/Campaign-FirstImage-5232.vsf",
            DetailUrl = "https://www.worldcard.com.tr/kampanyalar/opet-worldcard-ile-arac-yikama-sektorunde-200-tl-ve-uzeri-harcamalarinizda10-tl-toplamda-30-tl-opet-puan-90000198540/90000198540"
        });

        source.Add(new Campaign
        {
            Brand = "WorldCard",
            Sector = "Beyaz Eşya ve Ev Aletleri",
            Title = "Arçelik’ te peşin fiyatına 9 aya varan taksit fırsatı!",
            ImageUrl = "https://www.worldcard.com.tr/medium/Campaign-FirstImage-5245.vsf",
            DetailUrl = "https://www.worldcard.com.tr/kampanyalar/arcelik-te-pesin-fiyatina-9-aya-varan-taksit-firsati-90000198048/90000198048"
        });

        source.Add(new Campaign
        {
            Brand = "Maximum",
            Sector = "MARKET",
            Title = "Market Alışverişlerinize 200 TL'ye Varan MaxiPuan",
            ImageUrl = "https://www.maximum.com.tr/contentmanagement/PublishingImages/BAN-5151_YuksekTutar_Market_580x460.jpg",
            DetailUrl = "https://www.maximum.com.tr/kampanyalar/maximum-kartinizla-market-alisverinize-maxipuan-firsati"
        });

        source.Add(new Campaign
        {
            Brand = "Maximum",
            Sector = "GİYİM-AKSESUAR",
            Title = "Maximum'dan Peşin Giyim Alışverişlerinize Faizsiz 4 Taksit Fırsatı!",
            ImageUrl = "https://www.maximum.com.tr/contentmanagement/PublishingImages/BAN-5069_Giyim_Taksitlendirme_580x460.jpg",
            DetailUrl = "https://www.maximum.com.tr/kampanyalar/maximum-dan-pesin-giyim-alisverislerinize-faizsiz-4-taksit-firsati"
        });
    }
}

