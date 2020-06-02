using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Agents.Edge;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using MyWallet.Configuration;
using MyWallet.Framework.Services;
using MyWallet.Services.Interfaces;
using Microsoft.Extensions.Options;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using MyWallet.Extensions;
using MyWallet.Events;
using System.Reactive.Linq;
using Hyperledger.Indy;
using Hyperledger.Aries;
using System.Diagnostics;

namespace MyWallet.ViewModels.Connections
{
    public class ConnectionsViewModel : ABaseViewModel
    {
        private readonly IEdgeProvisioningService _edgeProvisioningService;
        private readonly IWalletAppConfiguration _walletConfiguration;
        private readonly AgentOptions _options;
        private CloudWalletService _cloudWalletService;
        private readonly IAgentProvider _agentProvider;
        private readonly IConnectionService _connectionService;
        private readonly ILifetimeScope _scope;
        private readonly IEventAggregator _eventAggregator;

        public ConnectionsViewModel(IUserDialogs userDialogs,
                                   INavigationService navigationService,
                                   IConnectionService connectionService,
                                   IEdgeProvisioningService edgeProvisioningService,
                                   IWalletAppConfiguration walletconfiguration,
                                   IOptions<AgentOptions> options,
                                   IAgentProvider agentProvider,
                                   ILifetimeScope scope,
                                   IEventAggregator eventAggregator,
                                   CloudWalletService cloudWalletService ) :
                                   base(nameof(ConnectionsViewModel), userDialogs, navigationService)
        {
            _edgeProvisioningService = edgeProvisioningService;
            _walletConfiguration = walletconfiguration;
            _options = options.Value;
            _cloudWalletService = cloudWalletService;
            _connectionService = connectionService;
            _agentProvider = agentProvider;
            _eventAggregator = eventAggregator;
            _scope = scope;
            Title = "Connections";
            _refreshingConnections = false;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await RefreshConnectionsList();
            _eventAggregator.GetEventByType<ApplicationEvent>()
                            .Where(_ => _.Type == ApplicationEventType.ConnectionsUpdated)
                            .Subscribe(async _ => await RefreshConnectionsList());
            await base.InitializeAsync(navigationData);
            RefreshingConnections = false;
        }


        public async Task RefreshConnectionsList()
        {
            try
            {
                RefreshingConnections = true;
                var context = await _agentProvider.GetContextAsync();
                if (context != null)
                {
                    var records = await _connectionService.ListAsync(context);

                    IList<ConnectionViewModel> connectionViewModels = new List<ConnectionViewModel>();

                    foreach (var record in records)
                    {
                        if (record.Alias != null)
                        {
                            var connection = _scope.Resolve<ConnectionViewModel>(new NamedParameter("record", record));
                            connectionViewModels.Add(connection);
                        }

                    }

                    Connections.Clear();
                    HasConnections = connectionViewModels.Any();

                    foreach (var connectionVm in connectionViewModels)
                    {
                        Connections.Add(connectionVm);
                    }
                    RefreshingConnections = false;
                }
            }
            catch (IndyException e)
            {
                RefreshingConnections = false;
                UserDialogs.Instance.Alert("Some error occurs. Our team is working on it.");
                Debug.WriteLine($"Reject Error - Indy: {e.Message}");
            }
            catch (AriesFrameworkException e)
            {
                RefreshingConnections = false;
                UserDialogs.Instance.Alert("Some error occurs. Our team is working on it.");
                Debug.WriteLine($"Reject Error - Aries: {e.Message}");
            }
            catch (Exception e)
            {
                RefreshingConnections = false;
                UserDialogs.Instance.Alert("Some error occurs. Our team is working on it.");
                Debug.WriteLine($"Reject Error - Xamarin: {e.Message}");
            }
            RefreshingConnections = false;
        }

        #region Bindable props

        private bool _refreshingConnections = false;

        public bool RefreshingConnections
        {
            get => _refreshingConnections;
            set => this.RaiseAndSetIfChanged(ref _refreshingConnections, value);
        }

        private bool _hasConnections;
        public bool HasConnections
        {
            get => _hasConnections;
            set => this.RaiseAndSetIfChanged(ref _hasConnections, value);
        }

        private RangeEnabledObservableCollection<ConnectionViewModel> _connections = new RangeEnabledObservableCollection<ConnectionViewModel>();

        public RangeEnabledObservableCollection<ConnectionViewModel> Connections
        {
            get => _connections;
            set => this.RaiseAndSetIfChanged(ref _connections, value);
        }

        #endregion

        #region Binable Command 
        public ICommand RefreshingCommand => new Command(async () => await RefreshConnectionsList());
        public ICommand GoToScanCommand => new Command(async () => await NavigationService.NavigateToAsync<ScanCodeViewModel>(null, Services.NavigationType.Modal));
        //public ICommand GoToScanCommand => new Command(async () => await NavigationService.NavigateToAsync<ScanCodeViewModel>());

        public ICommand OnSelectConnectionCommand => 
            new Command<ConnectionViewModel>(async (connectionVm) => {
                if(connectionVm != null)
                {
                    await NavigationService.NavigateToAsync<ConnectionViewModel>(connectionVm);
                }
            });
        #endregion
    }
}
