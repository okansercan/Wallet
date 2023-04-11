using System;
using Wallet.MAUI.ViewModel;

namespace Wallet.MAUI.Services
{
    public interface ICampaignService
    {
        Task<List<Campaign>> GetCampaignsAsync();
    }
}

