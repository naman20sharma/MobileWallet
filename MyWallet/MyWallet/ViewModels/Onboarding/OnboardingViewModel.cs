﻿using Acr.UserDialogs;
using Hyperledger.Aries.Agents.Edge;
using Hyperledger.Aries.Configuration;
using MyWallet.Configuration;
using MyWallet.Extensions;
using MyWallet.Framework.Services;
using MyWallet.Models.Onboarding;
using MyWallet.Services.Interfaces;
using Microsoft.Extensions.Options;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using FFImageLoading;
using MyWallet.ViewModels.Connections;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Routing;
using Hyperledger.Indy.WalletApi;
using Hyperledger.Aries.Storage;

namespace MyWallet.ViewModels.Onboarding
{
    public class OnboardingViewModel : ABaseViewModel
    {
        private int position;
        private readonly IEdgeProvisioningService _edgeProvisioningService;
        private readonly IWalletAppConfiguration _walletConfiguration;
        private readonly IEdgeClientService _edgeClientService;
        private AgentOptions _options;
        private readonly IAgentProvider _agentProvider;
        private readonly ILifetimeScope _scope;
        public OnboardingViewModel(IUserDialogs userDialogs,
            IEdgeClientService edgeClientService,
            INavigationService navigationService,
            IEdgeProvisioningService edgeProvisioningService,
            IAgentProvider agentProvider,
            ILifetimeScope scope,
            IWalletAppConfiguration walletconfiguration,
            IOptions<AgentOptions> options) :
            base("Onboarding", userDialogs, navigationService)
        {
            //services
            _edgeProvisioningService = edgeProvisioningService;
            _edgeClientService = edgeClientService;
            _walletConfiguration = walletconfiguration;
            _options = options.Value;
            _agentProvider = agentProvider;
            _scope = scope;
            SetSkipButtonText("SKIP");
            //SetNextButtontext("Next");
            SetNextButtonText("Create Wallet");
            InitializeOnBoarding();
            InitializeSkipCommand();
            InitializeNextCommand();
            //InitializeRetreiveWalletCommand();
        }

        private void SetSkipButtonText(string skipButtonText) => SkipButtonText = skipButtonText;

        private async Task<FingerprintAuthenticationResult> FingerprintAuthentication()
        {
            var authConfig = new AuthenticationRequestConfiguration("Fingerprint Authentication", "Please scan your Fingerprint for authentication. ")
            {
                FallbackTitle = "Use PIN",
                CancelTitle = "Cancel",
                AllowAlternativeAuthentication = true
            };
            return (await CrossFingerprint.Current.AuthenticateAsync(authConfig));
        }
        private async Task CreateAgent()
        {
            Preferences.Set(Constants.PoolConfigurationName, _walletConfiguration.PoolConfigurationName);
            var dialog = UserDialogs.Instance.Loading("Creating wallet");
            IsBusy = true;
            try
            {
                _options.AgentName = string.Format($"{DeviceInfo.Name} {AppInfo.Name}");
                Preferences.Set("AgentName", _options.AgentName);
                _options.WalletConfiguration.Id = Constants.LocalWalletIdKey;
                _options.WalletCredentials.Key = await SyncedSecureStorage.GetOrCreateSecureAsync(
                    key: Constants.LocalWalletCredentialKey,
                    value: Utils.Utils.GenerateRandomAsync(32));
                dialog?.Show();
                Preferences.Set("WalletConfigurationId", _options.WalletConfiguration.Id);
                Preferences.Set("WalletCredentialId", _options.WalletCredentials.Key);
                await _edgeProvisioningService.ProvisionAsync(_options);
                Preferences.Set("LocalWalletProvisioned", true);
                await Task.Delay(TimeSpan.FromSeconds(1));
                Preferences.Set("IsRefreshing", false);
                MessagingCenter.Send<OnboardingViewModel>(this, "SetupPushNotification");
                await NavigationService.NavigateToAsync<MainViewModel>();
                dialog?.Hide();
                dialog?.Dispose();
                dialog.TryDispose();
                UserDialogs.Instance.Alert("Wallet created successfully", "Info", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Wasn't able to provision the agent");
                dialog?.Hide();
                dialog?.Dispose();
                DialogService.Alert("Failed to create wallet!");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string _skipButtonText;
        public string SkipButtonText
        {
            get => _skipButtonText;
            set => this.RaiseAndSetIfChanged(ref _skipButtonText, value);
        }

        private string _nextButtonText;
        public string NextButtonText
        {
            get => _nextButtonText;
            set => this.RaiseAndSetIfChanged(ref _nextButtonText, value);
        }
        private void SetNextButtonText(string nextButtonText) => NextButtonText = nextButtonText;

        private RangeEnabledObservableCollection<OnboardingModel> _items = new RangeEnabledObservableCollection<OnboardingModel>();

        public RangeEnabledObservableCollection<OnboardingModel> Items
        {
            get => this._items;
            set => this.RaiseAndSetIfChanged(ref this._items, value);
        }

        private void InitializeOnBoarding()
        {
            IList<OnboardingModel> itemList = new List<OnboardingModel>()
            {
                new OnboardingModel
                {
                    //Title = "Welcome to \n Identify.Me",
                    Title = "Welcome to \n My Wallet",
                    Content = "A Wallet App for exchanging and storing verifiable credentials. ",
                    ImageUrl = "ob_blockchain_v1.svg"
                },
                //new OnboardingModel
                //{
                //    Title = "Control Data",
                //    Content = "Control your data by yourself",
                //    ImageUrl = "ob_control_data.svg"
                //},
                //new OnboardingModel
                //{
                //    Title = "Accept",
                //    Content = "Accept our term and service",
                //    ImageUrl = "ob_agree_term.svg"
                //}
            };
            Items.InsertRange(itemList);
        }

        private async void InitializeSkipCommand()
        {
            SkipCommand = new Command(async () =>
            {
                if (LastPositionReached())
                {
                    await this.CreateAgent();
                }
                else
                {
                    MoveToNextPosition();
                }
            });
        }

        private void InitializeNextCommand()
        {
            NextCommand = new Command(async () =>
            {
                if (LastPositionReached())
                {
                    var val2 = await CrossFingerprint.Current.GetAvailabilityAsync(true);
                    if (await CrossFingerprint.Current.IsAvailableAsync())
                    {
                        var authResult = await this.FingerprintAuthentication();
                        if (authResult.Authenticated || authResult.Status == FingerprintAuthenticationResultStatus.Succeeded)
                        {
                            await CreateAgent();
                        }
                        else if (authResult.Status.Equals(FingerprintAuthenticationResultStatus.Failed))
                        {
                            UserDialogs.Instance.Alert(authResult.ErrorMessage, "Info", "Ok");
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        await this.CreateAgent();
                    }
                }
                else
                {
                    MoveToNextPosition();
                }
            });
        }

        //private void BiometricMessage(FingerprintAuthenticationResult result)
        //{
        //    var authMessage = new Tuple<bool, string>(false, "");
        //    switch (result.Status)
        //    {
        //        case FingerprintAuthenticationResultStatus.Canceled:
        //            var asa  = true;
        //            var dsadhj = "";

        //            break;
        //        case FingerprintAuthenticationResultStatus.

        //        default:
        //    }
        //}

        private void MoveToNextPosition()
        {
            var nextPosition = ++Position;
            Position = nextPosition;
        }

        private async Task RetreiveWallet()
        {
            Preferences.Set(Constants.PoolConfigurationName, _walletConfiguration.PoolConfigurationName);
            var dialog = UserDialogs.Instance.Loading("Importing Wallet");
            try
            {
                var externalDirectoryPath = Preferences.Get("ExternalDirectoryPath", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
                var backupPath = Path.Combine(externalDirectoryPath, ".Indy_Backups", "Export_Wallet");
                if (!File.Exists(backupPath))
                {
                    dialog?.Hide();
                    dialog?.Dispose();
                    dialog?.TryDispose();
                    Device.BeginInvokeOnMainThread(async() =>
                     await DialogService.AlertAsync("Please create a new Wallet as there is no existing wallet backup.",
                        title: "Wallet Not Found")
                    );
                    return;
                }

                _options.AgentName = Preferences.Get("AgentName", DeviceInfo.Manufacturer + DeviceInfo.Name);
                _options.WalletConfiguration.Id = Constants.LocalWalletIdKey;
                _options.WalletCredentials.Key = await SyncedSecureStorage.GetOrCreateSecureAsync(
                    key: Constants.LocalWalletCredentialKey,
                    value: Utils.Utils.GenerateRandomAsync(32));
                dialog?.Show();
                string passPhrase = "ottava swill sniggle saint ungula bombing";
                var key = this.CreatePassphraseEncryption(passPhrase.Trim());
                var backupConfig = new WalletBackupConfig
                {
                    path = backupPath,
                    key = key
                };

                var importJson = JsonConvert.SerializeObject(backupConfig, Formatting.Indented);
                Debug.WriteLine(importJson);


                await Wallet.ImportAsync(_options.WalletConfiguration.ToJson(), _options.WalletCredentials.ToJson(), importConfig: importJson);
                
                Preferences.Set("LocalWalletProvisioned", true);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await NavigationService.NavigateToAsync<MainViewModel>();
                dialog?.Hide();
                dialog?.Dispose();
                dialog?.TryDispose();
                await UserDialogs.Instance.AlertAsync("Wallet successfully imported", "Info");
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
                dialog?.Hide();
                dialog?.Dispose();
                DialogService.Alert("Failed to import wallet!");
            }
        }
        private string CreatePassphraseEncryption(string passPhrase)
        {
            string hash = string.Empty;
            using (var sha256 = SHA256.Create())
            {
                Debug.WriteLine($"Hash: {passPhrase.GetHashCode()} UTF8: {passPhrase.GetUTF8Bytes()}");
                byte[] hashValue = sha256.ComputeHash(passPhrase.GetUTF8Bytes());
                hash = hashValue.GetUTF8String();
            }
            return hash;
        }
        private void InitializeRetreiveWalletCommand()
        {
            RetreiveWalletCommand = new Command(async () =>
            {
                var res = await CrossFingerprint.Current.GetAvailabilityAsync(allowAlternativeAuthentication: true);
                if(await CrossFingerprint.Current.IsAvailableAsync(allowAlternativeAuthentication: true))
                {
                    var authResult = await FingerprintAuthentication();
                    if (authResult.Authenticated || authResult.Status.Equals(FingerprintAuthenticationResultStatus.Succeeded))
                        await this.RetreiveWallet();
                    else if (authResult.Status.Equals(FingerprintAuthenticationResultStatus.Failed))
                        UserDialogs.Instance.Alert(authResult.ErrorMessage, "Info", "Ok");
                }
            });
        }
        public int Position
        {
            get => position;
            set
            {
                this.RaiseAndSetIfChanged(ref position, value);
                UpdateSkipButtonText();
                UpdateNextButtonText();
            }
        }

        private bool LastPositionReached()
           => Position == Items.Count - 1;

        private void UpdateSkipButtonText()
        {
            if (LastPositionReached())
            {
                SetSkipButtonText("GOT IT");
            }
            else
            {
                SetSkipButtonText("SKIP");
            }
        }

        private void UpdateNextButtonText()
        {
            if (LastPositionReached())
            {
                SetNextButtonText("Create Wallet");
                //SetNextButtontext("Continue");
            }
            else
            {
                SetNextButtonText("Next");
            }
        }
        private string ConvertStringtoMD5(string strword)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(strword);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private async Task RestoreBackup()
        {
            var dialog = UserDialogs.Instance.Loading("Restoring wallet");
            try
            {
                //AgentOptions options = new AgentOptions();
                Preferences.Set(Constants.PoolConfigurationName, _walletConfiguration.PoolConfigurationName);
                //_options.AgentName = string.Format($"{DeviceInfo.Name} {AppInfo.Name}");
                //_options.WalletConfiguration.Id = Constants.LocalWalletIdKey;
                //_options.WalletCredentials.Key = await SyncedSecureStorage.GetOrCreateSecureAsync(
                //    key: Constants.LocalWalletCredentialKey,
                //    value: Utils.Utils.GenerateRandomAsync(32));
                ////options.EndpointUri = _walletConfiguration.AgentFrameworkEndpoint;
                dialog?.Show();
                //await Wallet.CreateWalletAsync(_options.WalletConfiguration.ToString(), _options.WalletCredentials.ToString());
                await _edgeProvisioningService.ProvisionAsync();
                var context = await _scope.Resolve<IAgentProvider>().GetContextAsync();
                //var context = await _agentProvider.GetContextAsync();
                string passPhrase = "ottava swill sniggle saint ungula bombing";
                var seed = this.ConvertStringtoMD5(passPhrase);
                var agentOptions = await _edgeClientService.RestoreFromBackupAsync(context, seed: seed);
                var wallet = await _scope.Resolve<IWalletService>().GetWalletAsync(agentOptions.WalletConfiguration, agentOptions.WalletCredentials);
                //await Wallet.OpenWalletAsync(agentOptions.WalletConfiguration.ToString(), agentOptions.WalletCredentials.ToString());
                _options = agentOptions;
                //await _edgeProvisioningService.ProvisionAsync(agentOptions);
                //_options = agentOptions;
                Preferences.Set("WalletConfigurationId", agentOptions.WalletConfiguration.Id);
                Preferences.Set("WalletCredentialId", agentOptions.WalletCredentials.Key);
                System.Diagnostics.Debug.WriteLine(agentOptions);
                Preferences.Set("LocalWalletProvisioned", true);
                await Task.Delay(TimeSpan.FromSeconds(1));
                Preferences.Set("IsRefreshing", false);
                MessagingCenter.Send<OnboardingViewModel>(this, "SetupPushNotification");
                await NavigationService.NavigateToAsync<MainViewModel>();
                dialog?.Hide();
                dialog?.Dispose();
                dialog.TryDispose();
                UserDialogs.Instance.Alert("Wallet restored successfully", "Info", "OK");

            }
            catch (Exception e)

            {
                System.Diagnostics.Debug.WriteLine("Backup Error: {0}", e);
                dialog?.Hide();
                dialog?.Dispose();
                dialog.TryDispose();
                throw;
            }
        }
        
        public ICommand RestoreBackupCommand => new Command(async () => await this.RestoreBackup());

        public ICommand SkipCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand RetreiveWalletCommand { get; private set; }
    }
}
