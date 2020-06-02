using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using MyWallet.Extensions;
using MyWallet.Models.Setting;
using MyWallet.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MyWallet.ViewModels.Setting
{
    public class SettingViewModel : ABaseViewModel
    {
        public SettingViewModel(IUserDialogs userDialogs, 
            INavigationService navigationService) : 
            base (nameof(SettingViewModel), userDialogs, navigationService)
        {
            Title = "Setting";
            IList<NetworkItem> networkItems = new List<NetworkItem>()
            {                
                new NetworkItem("Bcovrin Test", "bcovrin-test"),
                new NetworkItem("Bcovrin Green Dev", "bcovrin-dev"),
                new NetworkItem("Infy Network Test", "infyNetwork-test")
            };
            _networkItems.InsertRange(networkItems);
            string selectedNetwork = Preferences.Get("PoolConfigurationName", "");
            _selectedNetworkItem = networkItems.SingleOrDefault(s => s.PoolName == selectedNetwork);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            
            await base.InitializeAsync(navigationData);         
        }

        private NetworkItem _selectedNetworkItem;

        public NetworkItem SelectedNetworkItem
        {
            get => _selectedNetworkItem;
            set 
            {
                this.RaiseAndSetIfChanged(ref _selectedNetworkItem, value);
                Preferences.Set("PoolConfigurationName", _selectedNetworkItem.PoolName);
            }
        }

        private RangeEnabledObservableCollection<NetworkItem> _networkItems = new RangeEnabledObservableCollection<NetworkItem>();

        public RangeEnabledObservableCollection<NetworkItem> NetworkItems
        {
            get => this._networkItems;
            set => this.RaiseAndSetIfChanged(ref _networkItems, value);
        }


    }
}
