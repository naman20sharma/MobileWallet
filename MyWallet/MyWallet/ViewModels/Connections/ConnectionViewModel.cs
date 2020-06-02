using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.Discovery;
using MyWallet.Extensions;
using MyWallet.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MyWallet.ViewModels.Connections
{
    public class ConnectionViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentProvider;
        private readonly IMessageService _messageService;
        private readonly IDiscoveryService _discoveryService;
        private readonly IConnectionService _connectionService;
        private readonly IEventAggregator _eventAggregator;
        Helpers.SomeMaterialColor someMaterialColor;

        public ConnectionViewModel(IUserDialogs userDialogs,
            INavigationService navigationService,
            IAgentProvider agentProvider,
            IMessageService messageService,
            IDiscoveryService discoveryService,
            IConnectionService connectionService,
            IEventAggregator eventAggregator,
            ConnectionRecord record) : base (nameof(ConnectionsViewModel), userDialogs, navigationService)
        {
            _agentProvider = agentProvider;
            _messageService = messageService;
            _discoveryService = discoveryService;
            _connectionService = connectionService;
            _eventAggregator = eventAggregator;
            _record = record;
            someMaterialColor = new Helpers.SomeMaterialColor();

            MyDid = _record.MyDid;
            TheirDid = _record.TheirDid;
            ConnectionName = _record.Alias.Name;
            ConnectionSubtitle = $"{_record.State:G}";
            Title = "Connection Detail";
            if (this._connectionImageUrl == null)
                _connectionImageUrl = $"https://ui-avatars.com/api/?name={_connectionName}&length=1&background={_organizeColor}&color=fff&size=128";
            else
                _connectionImageUrl = _record.Alias.ImageUrl;
            if (_record.CreatedAtUtc != null)
            {
                _createdDate = (DateTime)_record.CreatedAtUtc;
            }
        }

        private readonly ConnectionRecord _record;
        public ConnectionRecord Record
        {
            get => this._record;
        }

        #region Bindable Properties
        private string _connectionName;
        public string ConnectionName
        {
            get => _connectionName;
            set 
            { 
                this.RaiseAndSetIfChanged(ref _connectionName, value);
                _organizeColor = someMaterialColor.GetColorFromString(_connectionName);
                
            }
        }

        private string _myDid;
        public string MyDid
        {
            get => _myDid;
            set => this.RaiseAndSetIfChanged(ref _myDid, value);
        }

        private string _theirDid;
        public string TheirDid
        {
            get => _theirDid;
            set => this.RaiseAndSetIfChanged(ref _theirDid, value);
        }

        private string _connectionImageUrl;
        public string ConnectionImageUrl
        {
            get => _connectionImageUrl;
            set => this.RaiseAndSetIfChanged(ref _connectionImageUrl, value);
        }

        private DateTime _createdDate = DateTime.Now;
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => this.RaiseAndSetIfChanged(ref _createdDate, value);
        }

        private string _connectionSubtitle = "Lorem ipsum dolor sit amet";
        public string ConnectionSubtitle
        {
            get => _connectionSubtitle;
            set => this.RaiseAndSetIfChanged(ref _connectionSubtitle, value);
        }

        private RangeEnabledObservableCollection<TransactionItem> _transactions = new RangeEnabledObservableCollection<TransactionItem>();
        public RangeEnabledObservableCollection<TransactionItem> Transactions
        {
            get => _transactions;
            set => this.RaiseAndSetIfChanged(ref _transactions, value);
        }

        private bool _refreshingTransactions;
        public bool RefreshingTransactions
        {
            get => _refreshingTransactions;
            set => this.RaiseAndSetIfChanged(ref _refreshingTransactions, value);
        }

        private bool _hasTransactions;
        public bool HasTransactions
        {
            get => _hasTransactions;
            set => this.RaiseAndSetIfChanged(ref _hasTransactions, value);
        }

        private string _organizeColor = "009688";
        public string OrganizeColor
        {
            get => _organizeColor;
            set => this.RaiseAndSetIfChanged(ref _organizeColor, value);
        }
        #endregion


        #region bindable command
        ICommand OnSelectDeleleButtonCommad => new Command(async () => {
            var context = await _agentProvider.GetContextAsync();
        });
        #endregion
    }
}
