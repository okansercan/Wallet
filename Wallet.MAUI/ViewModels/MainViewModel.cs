using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wallet.MAUI.Services;

namespace Wallet.MAUI.ViewModel;

public partial class MainViewModel : ObservableObject
{
    IList<Campaign> source;

    public MainViewModel()
    {
        Campaigns = new ObservableCollection<Campaign>();
        Brands = new ObservableCollection<string>();
        Sectors = new ObservableCollection<string>();
        source = new List<Campaign>();
        GetCampaignCollection();
    }

    [ObservableProperty]
    ObservableCollection<Campaign> campaigns;

    [ObservableProperty]
    ObservableCollection<string> brands;

    [ObservableProperty]
    private int brandIndex = -1;

    [ObservableProperty]
    ObservableCollection<string> sectors;

    [ObservableProperty]
    private int sectorIndex = -1;

    [ObservableProperty]
    string text;

    partial void OnBrandIndexChanged(int value) {
        if (value == -1)
            return;

        GetBrandSectors(value);
        GetBrandCampaigns(value);
        Text = string.Empty;
    }

    partial void OnSectorIndexChanged(int value)
    {
        if (value == -1)
            return;

        GetSectorCampaigns(value);
    }

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

        BrandIndex = -1;
        SectorIndex = -1;
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

    private async void GetCampaignCollection()
    {
        ICampaignService service = new CampaignService();
        source = await service.GetCampaignsAsync();

        foreach(string brand in source.Select(campaign => campaign.Brand).Distinct())
        {
            Brands.Add(brand);
        }
    }

    private void GetBrandSectors(int brandIndex)
    {
        string brandName = Brands.ElementAt<string>(brandIndex);
        Sectors.Clear();
        foreach (var sector in source.Where(c => c.Brand == brandName).Select(s => s.Sector).Distinct())
        {
            Sectors.Add(sector);
        }
    }

    private void GetBrandCampaigns(int brandIndex)
    {
        string brandName = Brands.ElementAt<string>(brandIndex);

        var filteredItems = source.Where(campaign => campaign.Brand == brandName).ToList();

        Campaigns.Clear();
        foreach (var campaign in filteredItems)
        {
            Campaigns.Add(campaign);
        }
    }

    private void GetSectorCampaigns(int sectorIndex)
    {
        string brandName = Brands.ElementAt<string>(BrandIndex);
        string sectorName = Sectors.ElementAt<string>(sectorIndex);

        var filteredItems = source.Where(campaign => campaign.Brand == brandName && campaign.Sector == sectorName).ToList();

        Campaigns.Clear();
        foreach (var campaign in filteredItems)
        {
            Campaigns.Add(campaign);
        }
    }
}

