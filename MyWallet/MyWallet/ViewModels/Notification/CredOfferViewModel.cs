using Acr.UserDialogs;
using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Storage;
using Hyperledger.Indy;
using MyWallet.Events;
using MyWallet.Framework.Services;
using MyWallet.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace MyWallet.ViewModels.Notification
{
    public class CredOfferViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentProvider;
        private readonly ICredentialService _credentialService;
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly IPoolService _poolService;
        private readonly IWalletRecordService _recordService;
        private readonly IEventAggregator _eventAggregator;

        public CredOfferViewModel(IUserDialogs userDialogs,
            INavigationService navigationService,
            IAgentProvider agentProvider,
            ICredentialService credentialService,
            IMessageService messageService,
            IPoolService poolService,
            IWalletRecordService recordService,
            IConnectionService connectionService,
            IEventAggregator eventAggregator, CredentialRecord credentialOffer) :
            base(nameof(CredOfferViewModel), userDialogs, navigationService)
        {
            _credentialOffer = credentialOffer;

            _agentProvider = agentProvider;
            _credentialService = credentialService;
            _connectionService = connectionService;
            _messageService = messageService;
            _poolService = poolService;
            _recordService = recordService;
            _eventAggregator = eventAggregator;
            Title = "Offer Detail";
            CredentialName = ConvertNameFromeSchemaId(CredentialOffer.SchemaId);

            if (_credentialOffer.CreatedAtUtc != null)
            {
                IssuedDate = ((DateTime)_credentialOffer.CreatedAtUtc).ToLocalTime();
            }
        }
        private string ConvertNameFromeSchemaId(string schemaId)
        {
            var arr = schemaId.Split(new string[] { ":" }, StringSplitOptions.None);
            if (arr[2] != null)
            {
                return arr[2];
            }
            return "NoName";
        }

        #region Bindable Props 
        private CredentialRecord _credentialOffer;
        public CredentialRecord CredentialOffer
        {
            get => _credentialOffer;
            set
            {
                this.RaiseAndSetIfChanged(ref _credentialOffer, value);
                if (value != null)
                {
                    _credentialAttributes = value.CredentialAttributesValues;
                }

            }
        }

        private string _credentialName;
        public string CredentialName
        {
            get => _credentialName;
            set => this.RaiseAndSetIfChanged(ref _credentialName, value);
        }

        private IEnumerable<CredentialPreviewAttribute> _credentialAttributes;

        public IEnumerable<CredentialPreviewAttribute> CredentialData
        {
            get => _credentialAttributes;
            set => this.RaiseAndSetIfChanged(ref _credentialAttributes, value);
        }

        private DateTime _issuedDate = DateTime.Now;
        public DateTime IssuedDate
        {
            get => _issuedDate;
            set => this.RaiseAndSetIfChanged(ref _issuedDate, value);
        }
        #endregion

        private async Task AcceptCredentialOffer()
        {
            if (this.CredentialOffer != null)
            {
                var context = await _agentProvider.GetContextAsync();
                var loadingDialog = DialogService.Loading("Proccessing");
                try
                {
                    var poolConfigName = Preferences.Get(Constants.PoolConfigurationName, "bcovrin-test");
                    var a = await _poolService.GetPoolAsync(poolConfigName, 2);
                    var (requestMessage, credRecord) = await _credentialService.CreateRequestAsync(context, CredentialOffer.Id);
                    var connectionRecord = await _connectionService.GetAsync(context, credRecord.ConnectionId);
                    await _messageService.SendAsync(context, requestMessage, connectionRecord);
                    loadingDialog.Hide();
                    await NavigationService.NavigateBackAsync();
                    var toastConfig = new ToastConfig("Accepted Credential Offer");
                    toastConfig.BackgroundColor = Color.Green;
                    toastConfig.Position = ToastPosition.Top;
                    toastConfig.SetDuration(3000);
                    DialogService.Toast(toastConfig);

                    _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialsUpdated });
                }
                catch (IndyException e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                    loadingDialog.Hide();
                    await NavigationService.NavigateBackAsync();
                    if (e.SdkErrorCode == 309)
                        await Application.Current.MainPage.DisplayAlert("Error", "Something wrong while connect to pool. Go to setting page to choose pool", "OK");

                }
                catch (AriesFrameworkException e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                    loadingDialog.Hide();
                    await NavigationService.NavigateBackAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", "Can not send A2A message. Check your connection", "OK");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                    loadingDialog.Hide();
                    await NavigationService.NavigateBackAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", "Unknow errow. Don't worrry, we are working on it.", "OK");
                }
            }
            //await _messageService.SendAsync<CredentialProposeMessage>()
            //await Application.Current.MainPage.DisplayAlert("Something error with this Offer", "", "Ok");

        }

        private async Task RejectCredentialOffer()
        {
            try
            {
                var result = await UserDialogs.Instance.ConfirmAsync("Are you sure you want to reject this offer?", "Confirm Rejection", "Yes", "No");
                if (result)
                {
                    var context = await _agentProvider.GetContextAsync();
                    await _credentialService.RejectOfferAsync(context, this._credentialOffer.Id);
                    await NavigationService.NavigateBackAsync();
                    _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialsUpdated });
                }
            }
            catch (IndyException e)
            {
                UserDialogs.Instance.Alert("Some error occurs. Our team is working on it.");
                Debug.WriteLine($"Reject Error - Indy: {e.Message}");
            }
            catch (AriesFrameworkException e)
            {
                UserDialogs.Instance.Alert("Some error occurs. Our team is working on it.");
                Debug.WriteLine($"Reject Error - Aries: {e.Message}");
            }
            catch (Exception e)
            {
                UserDialogs.Instance.Alert("Some error occurs. Our team is working on it.");
                Debug.WriteLine($"Reject Error - Xamarin: {e.Message}");
            }

        }

        #region Bindable Command
        public ICommand AcceptCredentialOfferCommand => new Command(async () => await AcceptCredentialOffer());
        public ICommand RejectCredentialOfferCommand => new Command(async () => await RejectCredentialOffer());
        #endregion
    }
}
