using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using MyWallet.Framework.Services;
using MyWallet.Messages;
using MyWallet.Services.Interfaces;
using Plugin.LocalNotification;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Hyperledger.Aries.Storage;
using System.Linq;
using Autofac;
using MyWallet.Extensions;
using Hyperledger.Aries.Contracts;
using MyWallet.Events;
using System.Reactive.Linq;
using Autofac.Core;
using System.Collections.Generic;

namespace MyWallet.ViewModels.Notification
{
    public class NotificationViewModel : ABaseViewModel
    {
        private ICredentialService _credentialService;
        private IConnectionService _connectionService;
        private IProofService _proofService;
        private IMessageService _messageService;
        private IAgentProvider _agentProvider;
        private CloudWalletService _cloudWalletService;
        private readonly ILifetimeScope _scope;
        private readonly IEventAggregator _eventAggregator;
        public NotificationViewModel(IUserDialogs userDialogs,
                                   INavigationService navigationService,
                                   IAgentProvider agentProvider,
                                   ICredentialService credentialService,
                                   IConnectionService connectionService,
                                   IMessageService messageService,
                                   CloudWalletService cloudWalletService,
                                   IProofService proofService, 
                                   ILifetimeScope scope, 
                                   IEventAggregator eventAggregator) : base (nameof(NotificationViewModel), userDialogs, navigationService)
        {
            _agentProvider = agentProvider;
            _credentialService = credentialService;
            _proofService = proofService;
            _connectionService = connectionService;
            _cloudWalletService = cloudWalletService;
            _messageService = messageService;
            _scope = scope;
            _eventAggregator = eventAggregator;
            Title = "Notification";
        }

        public async override Task InitializeAsync(object navigationData)
        {
           
            await base.InitializeAsync(navigationData);
            _eventAggregator.GetEventByType<ApplicationEvent>()
               .Where(_ => _.Type == ApplicationEventType.CredentialsUpdated)
               .Subscribe(async _ => await GetRequiredRecord());
            _eventAggregator.GetEventByType<ApplicationEvent>()
                .Where(_ => _.Type == ApplicationEventType.GotCredentialOffer)
                .Subscribe(async _ => await GetRequiredRecord());
            _eventAggregator.GetEventByType<ApplicationEvent>()
                .Where(_ => _.Type == ApplicationEventType.GotProofRequestMessage)
                .Subscribe(async _ => await GetRequiredRecord());
            _eventAggregator.GetEventByType<ApplicationEvent>()
                .Where(_ => _.Type == ApplicationEventType.NotificationUpdated)
                .Subscribe(async _ => await GetRequiredRecord());
            await GetRequiredRecord();
            IsRefreshing = false;
        }

        void HandleReceivedMessages()
        {
            MessagingCenter.Subscribe<TickedMessage>(this, "TickedMessage", message => {
                Device.BeginInvokeOnMainThread(async () => {
                    var num = await _cloudWalletService.FetchCloudMessagesAsync();
                    if (num != 0)
                    {
                        var notification = new NotificationRequest
                        {
                            NotificationId = 100,
                            Title = "Message from mediator",
                            Description = $"You've got {num} message from mediator",
                            ReturningData = "Dummy data", // Returning data when tapped on notification.
                                                          //NotifyTime = DateTime.Now.AddSeconds(30) // Used for Scheduling local notification, if not specified notification will show immediately.
                        };
                        NotificationCenter.Current.Show(notification);
                    }
                });
            });

        }

        private async Task GetRequiredRecord()
        {
            try
            {
                IsRefreshing = true;
                var agentContext = await _agentProvider.GetContextAsync();
                //ISearchQuery credentialsQuery = ListOffersAsync
                var listCredentials = await _credentialService.ListOffersAsync(agentContext);
                var listProofRequests = await _proofService.ListRequestedAsync(agentContext);
                IList<NotificationItem> notificationItemList = new List<NotificationItem>();

                //old version of notification
                //IList<CredOfferViewModel> credOfferViewModels = new List<CredOfferViewModel>();
                foreach (var item in listCredentials)
                {
                    CredOfferViewModel credOfferViewModel = _scope.Resolve<CredOfferViewModel>(new NamedParameter("credentialOffer", item));
                    NotificationItem notificationItem = new NotificationItem()
                    {
                        NotificationType = "Credential Offer",
                        NotificationTitle = credOfferViewModel.CredentialName,
                        NotificationContent = "You've received a credentital offer.",
                        ItemViewModel = credOfferViewModel,
                        IssuedDate = credOfferViewModel.IssuedDate
                    };

                    var connection = await _connectionService.GetAsync(agentContext, item.ConnectionId);
                    notificationItem.OrganizeAlias = connection.Alias;

                    //old version of notification
                    //credOfferViewModels.Add(credOfferViewModel);
                    notificationItemList.Add(notificationItem);
                }

                //old version of notification
                //IList<ProofRequestViewModel> proofRequestViewModels = new List<ProofRequestViewModel>();
                foreach (var item in listProofRequests)
                {
                    ProofRequestViewModel proofRequestVm = _scope.Resolve<ProofRequestViewModel>(new NamedParameter("proofRequestRecord", item));
                    NotificationItem notificationItem = new NotificationItem()
                    {
                        NotificationType = "Proof Request",
                        NotificationTitle = proofRequestVm.ProofRequestObject.Name,
                        NotificationContent = "You've received a request for proof of credential.",
                        ItemViewModel = proofRequestVm,
                        IssuedDate = proofRequestVm.IssuedDate
                    };

                    var connection = await _connectionService.GetAsync(agentContext, item.ConnectionId);
                    notificationItem.OrganizeAlias = connection.Alias;

                    //old version of notification
                    //proofRequestViewModels.Add(proofRequestVm);
                    notificationItemList.Add(notificationItem);
                }
                //old version of notification
                //_proofRequestVms.Clear();
                //_proofRequestVms.InsertRange(proofRequestViewModels);

                //_credentialOfferVms.Clear();
                //_credentialOfferVms.InsertRange(credOfferViewModels);
                //--end old version of notification
                var orderList = notificationItemList.OrderBy(item => item.IssuedDate);
                NotificationItems.Clear();
                NotificationItems.InsertRange(orderList);
                IsRefreshing = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Notification Error: " + e.Message);
                IsRefreshing = false;
            }

        }

        public async Task NavigateToCredentialOfferPage(CredOfferViewModel credentialOfferVm)
        {
            await NavigationService.NavigateToAsync<CredOfferViewModel>(credentialOfferVm);
        }

        public ICommand SelectCredOfferCommand => new Command<CredOfferViewModel>(async (credOfferViewModel) =>
        {
            if (credOfferViewModel != null)
                await NavigateToCredentialOfferPage(credOfferViewModel);
        });

        public async Task NavigateToProofRequestPage(ProofRequestViewModel proofRequestViewModel)
        {
            await NavigationService.NavigateToAsync<ProofRequestViewModel>(proofRequestViewModel);
        }

        #region Bindable Props 


        private bool _isRefreshing = false;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => this.RaiseAndSetIfChanged(ref _isRefreshing, value);
        }

        private RangeEnabledObservableCollection<ReactiveObject> _notificationItems = new RangeEnabledObservableCollection<ReactiveObject>();

        public RangeEnabledObservableCollection<ReactiveObject> NotificationItems
        {
            get => this._notificationItems; 
            set => this.RaiseAndSetIfChanged(ref _notificationItems, value);
        }

        //old version of notification

        //private RangeEnabledObservableCollection<CredOfferViewModel> _credentialOfferVms = new RangeEnabledObservableCollection<CredOfferViewModel>();
        //public RangeEnabledObservableCollection<CredOfferViewModel> CredOffers
        //{
        //    get => _credentialOfferVms;
        //    set => this.RaiseAndSetIfChanged(ref _credentialOfferVms, value);
        //}

        //private RangeEnabledObservableCollection<ProofRequestViewModel> _proofRequestVms = new RangeEnabledObservableCollection<ProofRequestViewModel>();
        //public RangeEnabledObservableCollection<ProofRequestViewModel> ProofRequests
        //{
        //    get => _proofRequestVms;
        //    set => this.RaiseAndSetIfChanged(ref _proofRequestVms, value);
        //}     

        #endregion

        #region Bindable Command
        public ICommand RefreshRecordCommand => new Command(async () => await GetRequiredRecord());

        public ICommand SelectNotificationItemCommand => new Command<NotificationItem>(async (item) =>
        {
            if (item != null)
            { 
                if(item.ItemViewModel is ProofRequestViewModel)
                {
                    var viewModel = (ProofRequestViewModel)item.ItemViewModel;
                    await NavigateToProofRequestPage(viewModel);
                }
                if(item.ItemViewModel is CredOfferViewModel)
                {
                    var viewModel = (CredOfferViewModel)item.ItemViewModel;
                    await NavigateToCredentialOfferPage(viewModel);
                }
            }
        });

        public ICommand SelectProofRequestCommand => new Command<ProofRequestViewModel>(async (proofRequestViewModel) =>
        {
            if (proofRequestViewModel != null)
                await NavigateToProofRequestPage(proofRequestViewModel);
        });
        #endregion

    }
}
