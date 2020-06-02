using Acr.UserDialogs;
using Hyperledger.Aries.Agents.Edge;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Decorators.Transport;
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
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MyWallet.ViewModels.Onboarding
{
    public class OnboardingViewModel : ABaseViewModel
    {
        private int position;
        private readonly IEdgeProvisioningService _edgeProvisioningService;
        private readonly IWalletAppConfiguration _walletConfiguration;
        private readonly AgentOptions _options;

        public OnboardingViewModel(IUserDialogs userDialogs,
            INavigationService navigationService,
            IEdgeProvisioningService edgeProvisioningService,
            IWalletAppConfiguration walletconfiguration,
            IOptions<AgentOptions> options) :
            base("Onboarding", userDialogs, navigationService)
        {
            //services
            _edgeProvisioningService = edgeProvisioningService;
            _walletConfiguration = walletconfiguration;
            _options = options.Value;

            SetSkipButtonText("SKIP");
            //SetNextButtontext("Next");
            SetNextButtonText("Create Wallet");
            InitializeOnBoarding();
            InitializeSkipCommand();
            InitializeNextCommand();
        }

        private void SetSkipButtonText(string skipButtonText) => SkipButtonText = skipButtonText;

        private async Task<FingerprintAuthenticationResult> FingerprintAuthentication()
        {
            var authConfig = new AuthenticationRequestConfiguration("Fingerprint Authentication", "Please scan your Fingerprint for authentication. ") {
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
                _options.AgentName = DeviceInfo.Name + " " + AppInfo.Name;
                _options.WalletConfiguration.Id = Constants.LocalWalletIdKey;
                _options.WalletCredentials.Key = await SyncedSecureStorage.GetOrCreateSecureAsync(
                    key: Constants.LocalWalletCredentialKey,
                    value: Utils.Utils.GenerateRandomAsync(32));
                await _edgeProvisioningService.ProvisionAsync(_options);
                Preferences.Set("LocalWalletProvisioned", true);
                await NavigationService.NavigateToAsync<MainViewModel>();
                dialog?.Hide();
                dialog?.Dispose();
                DialogService.Alert("Wallet created successfully", "Info", "OK");
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

        public ICommand SkipCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
    }
}
