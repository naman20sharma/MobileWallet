using Acr.UserDialogs;
using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.BasicMessage;

using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.Discovery;
using Hyperledger.Aries.Storage;
using MyWallet.Events;
using MyWallet.Extensions;
using MyWallet.Services.Interfaces;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MyWallet.ViewModels.Connections
{
    public class ConnectionViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentProvider;
        private readonly IMessageService _messageService;
        private readonly IDiscoveryService _discoveryService;
        private readonly IWalletRecordService _walletRecordService;
        private readonly IConnectionService _connectionService;
        private readonly IEventAggregator _eventAggregator;
        Helpers.SomeMaterialColor someMaterialColor;

        public ConnectionViewModel(IUserDialogs userDialogs,
            INavigationService navigationService,
            IAgentProvider agentProvider,
            IMessageService messageService,
            IDiscoveryService discoveryService,
            IConnectionService connectionService,
            IWalletRecordService walletRecordService,
            IEventAggregator eventAggregator,
            ConnectionRecord record) : base(nameof(ConnectionsViewModel), userDialogs, navigationService)
        {
            _agentProvider = agentProvider;
            _messageService = messageService;
            _discoveryService = discoveryService;
            _walletRecordService = walletRecordService;
            _connectionService = connectionService;
            _eventAggregator = eventAggregator;
            _record = record;
            someMaterialColor = new Helpers.SomeMaterialColor();

            MyDid = _record.MyDid;
            TheirDid = _record.TheirDid;
            ConnectionName = _record.Alias.Name;
            ConnectionSubtitle = $"{_record.State:G}";
            Title = "Connection Detail";
            if (_record.Alias.ImageUrl == null)
                _connectionImageUrl = $"https://ui-avatars.com/api/?name={_connectionName}&length=1&background={_organizeColor}&color=fff&size=128";
            else
                _connectionImageUrl = _record.Alias.ImageUrl;
            if (_record.CreatedAtUtc != null)
            {
                _createdDate = ((DateTime)_record.CreatedAtUtc).ToLocalTime();
            }
        }

        private async Task DeleteConnection()
        {
            var result = await UserDialogs.Instance.ConfirmAsync("Are you sure you want to delete this connection, it will delete all credenetial records?", "Delete Connection", "Yes", "No");
            if (result)
            {
                var dialog = UserDialogs.Instance.Loading("Deleting");
                try
                {
                    var context = await _agentProvider.GetContextAsync();
                    var isDeleted = await _connectionService.DeleteAsync(context, Record.Id);
                    if (isDeleted)
                    {
                        Preferences.Set("DeletedConnection", Record.Id);
                        //_eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.DeleteAllCredentials });
                        _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionRemoved });
                    }
                    else
                        await DialogService.AlertAsync("Sorry, Could not delete the connection.", "Deletion Error");
                    if (dialog.IsShowing)
                    {
                        dialog?.Hide();
                        dialog?.Dispose();
                    }
                    await NavigationService.NavigateBackAsync();
                }
                catch (AriesFrameworkException e)
                {
                    if (dialog.IsShowing)
                    {
                        dialog?.Hide();
                        dialog?.Dispose();
                    }
                    DialogService.Alert($"Some error occured with Aries:{Environment.NewLine}  {e.Message}", "Error", "Ok");
                }
                catch (Exception e)
                {
                    if (dialog.IsShowing)
                    {
                        dialog?.Hide();
                        dialog?.Dispose();
                    }
                    DialogService.Alert($"Some error occured :{Environment.NewLine}  {e.Message}", "Error", "Ok");
                }
            }
        }

        private async Task SendMessage()
        {
            try
            {
                //var promptResult = await DialogService.PromptAsync(new PromptConfig
                //{
                //    InputType = InputType.Default,
                //    OkText = "Send",
                //    Title = "Message",
                //    IsCancellable = true,
                //    Placeholder = "Message",
                //});
                //if (promptResult.Ok && !string.IsNullOrEmpty(promptResult.Text))
                //{
                var jsonObj = new
                {
                    type = "fetch_service",
                    values = new Object()
                };
                string jsonData = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                Debug.WriteLine(jsonData);
                var context = await _agentProvider.GetContextAsync();
                var sentTime = DateTime.Now;
                var text = jsonData;
                var messageRecord = new BasicMessageRecord()
                {
                    Id = Guid.NewGuid().ToString(),
                    Direction = MessageDirection.Outgoing,
                    Text = text,
                    ConnectionId = Record.Id,
                    SentTime = sentTime.ToUniversalTime()
                };
                var message = new BasicMessage()
                {
                    Content = text,

                    SentTime = sentTime.ToString("s", CultureInfo.InvariantCulture)
                };

                var connection = await _connectionService.GetAsync(context, Record.Id);
                await _walletRecordService.AddAsync(context.Wallet, messageRecord);
                await _messageService.SendAsync(context.Wallet, message, connection);

                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                await DialogService.AlertAsync("Unable to send message, please try again later.", "Error");
                throw e;
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
        public ICommand OnSelectDeleleButtonCommad => new Command(async () =>
        {
            var context = await _agentProvider.GetContextAsync();
        });

        public ICommand OnSelectDeleteMenuItem =>
           new Command(async () =>
           {
               await this.DeleteConnection();
           });
        //public ICommand OnSelectSendMessageMenuItem =>
        //   new Command(async () =>
        //   {
        //       await this.SendMessage();
        //   });
        #endregion
    }
}
