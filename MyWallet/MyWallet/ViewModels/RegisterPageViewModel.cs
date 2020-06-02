using Acr.UserDialogs;
using Hyperledger.Aries.Agents.Edge;
using Hyperledger.Aries.Configuration;
using MyWallet.Configuration;
using MyWallet.Framework.Services;
using MyWallet.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MyWallet.ViewModels
{
    public class RegisterPageViewModel : ABaseViewModel
    {
        private readonly IEdgeProvisioningService _edgeProvisioningService;
        private readonly IWalletAppConfiguration _walletConfiguration;
        private readonly AgentOptions _options;
        private CloudWalletService _cloudWalletService;

        public RegisterPageViewModel(IUserDialogs userDialogs,
            INavigationService navigationService, 
            IEdgeProvisioningService edgeProvisioningService,
            IWalletAppConfiguration walletconfiguration,
            IOptions<AgentOptions> options,
            CloudWalletService cloudWalletService) : base (nameof(RegisterPageViewModel), userDialogs, navigationService)
        {
            _edgeProvisioningService = edgeProvisioningService;
            _walletConfiguration = walletconfiguration;
            _options = options.Value;
            _cloudWalletService = cloudWalletService;
            Title = "Register Wallet";
        }

        private async Task CreateAgent()
        {
            Preferences.Set(Constants.PoolConfigurationName, _walletConfiguration.PoolConfigurationName);
            var dialog = UserDialogs.Instance.Loading("Creating wallet");
            IsBusy = true;
            try
            {
                _options.AgentName = DeviceInfo.Name;
                _options.WalletConfiguration.Id = Constants.LocalWalletIdKey;
                _options.WalletCredentials.Key = await SyncedSecureStorage.GetOrCreateSecureAsync(
                    key: Constants.LocalWalletCredentialKey,
                    value: Utils.Utils.GenerateRandomAsync(32));
                await _edgeProvisioningService.ProvisionAsync(_options);
                Preferences.Set("LocalWalletProvisioned", true);
                await NavigationService.NavigateToAsync<MainViewModel>();
                dialog?.Hide();
                dialog?.Dispose();
                await Application.Current.MainPage.DisplayAlert("Wallet created", "", "Ok");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Wasn't able to provision the agent");
                //await Application.Current.MainPage.DisplayAlert("An error occured while registering the wallet", "", "Ok");
                dialog?.Hide();
                dialog?.Dispose();
                UserDialogs.Instance.Alert("Failed to create wallet!");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand CreateWalletCommand => new Command(async () => await CreateAgent());


    }
}
