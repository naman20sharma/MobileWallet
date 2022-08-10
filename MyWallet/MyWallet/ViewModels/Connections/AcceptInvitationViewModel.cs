using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using MyWallet.Events;
using MyWallet.Services.Interfaces;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MyWallet.ViewModels.Connections
{
    public class AcceptInvitationViewModel : ABaseViewModel
    {
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly IProvisioningService _provisioningService;
        private readonly IAgentProvider _mobileAgentProvider;
        private readonly IEventAggregator _eventAggregator;
        public AcceptInvitationViewModel(IUserDialogs userDialogs, 
            INavigationService navigationService,
            IConnectionService connectionService,
            IProvisioningService provisioningService, 
            IMessageService messageService, 
            IAgentProvider mobileAgentProvider, 
            IEventAggregator eventAggregator) : base(nameof(AcceptInvitation), userDialogs, navigationService)
        {
            _mobileAgentProvider = mobileAgentProvider;
            _provisioningService = provisioningService;
            _connectionService = connectionService;
            _messageService = messageService;
            _eventAggregator = eventAggregator;
            Title = "Invitation";
        }

        private async Task AcceptInvitation()
        {
            var loadingDialog = DialogService.Loading("Proccessing");
            this.IsBusy = true;
            if (_invitation != null)
            {
                try
                {
                    var agentContext = await _mobileAgentProvider.GetContextAsync();
                    if (agentContext == null)
                    {
                        loadingDialog.Hide();
                        DialogService.Alert("Failed to decode invitation!");
                        return;
                    }
                    var (requestMessage, connectionRecord) = await _connectionService.CreateRequestAsync(agentContext, _invitation);
                    //var provisioningRecord = await _provisioningService.GetProvisioningAsync(agentContext.Wallet);
                    //var isEndpointUriAbsent = provisioningRecord.Endpoint.Uri == null;

                    await _messageService.SendAsync(agentContext, requestMessage, connectionRecord);
                    //if (isEndpointUriAbsent)
                    //{
                    //    string processRes = await _connectionService.ProcessResponseAsync(agentContext, respone, connectionRecord);
                    //}
                    loadingDialog.Hide();
                    await NavigationService.CloseAllPopupsAsync();
                    var toastConfig = new ToastConfig("Connection Saved!");
                    toastConfig.BackgroundColor = Color.Green;
                    toastConfig.Position = ToastPosition.Top;
                    toastConfig.SetDuration(3000);
                    DialogService.Toast(toastConfig);
                    _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionsUpdated });

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                    loadingDialog.Hide();
                    DialogService.Alert("Something went wrong!");
                }

            }

        }

        private ConnectionInvitationMessage _invitation;
        public ConnectionInvitationMessage InvitationMessage
        {
            get => _invitation;
            set => this.RaiseAndSetIfChanged(ref _invitation, value);
        }

        #region Binding Command
        public ICommand AcceptInvitationCommand => new Command(async () => await AcceptInvitation());
        #endregion
    }
}
