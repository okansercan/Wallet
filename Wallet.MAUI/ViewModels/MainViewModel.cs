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

    private async void GetCampaignCollection()
    {
        ICampaignService service = new CampaignService();
        source = await service.GetCampaignsAsync();
    }
}

