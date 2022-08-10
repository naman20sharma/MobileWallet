using Acr.UserDialogs;
using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Indy;
using MyWallet.Events;
using MyWallet.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MyWallet.ViewModels.Credentials
{
    public class CredentialViewModel : ABaseViewModel
    {
        private readonly CredentialRecord _credential;
        private readonly IConnectionService _connectionService;
        private readonly IAgentProvider _agentProvider;
        private readonly ICredentialService _credentialService;
        private readonly IEventAggregator _eventAggregator;
        Helpers.SomeMaterialColor someMaterialColor;
        public CredentialViewModel(IUserDialogs userDialogs,
            INavigationService navigationService,
            CredentialRecord credential,
            IAgentProvider agentProvider,
            ICredentialService credentialService,
            IEventAggregator eventAggregator,
            IConnectionService connectionService) :
            base(nameof(CredentialViewModel), userDialogs, navigationService)
        {
            _connectionService = connectionService;
            _agentProvider = agentProvider;
            _credentialService = credentialService;
            _eventAggregator = eventAggregator;

            Title = "Credential Detail";
            _credential = credential;
            _attributes = credential.CredentialAttributesValues;
            _credId = _credential.Id;
            _state = _credential.State;
            _credentialName = ConvertNameFromeSchemaId(_credential.SchemaId).ToTitleCase();
            if (_credential.CreatedAtUtc != null)
            {
                IssuedDate = ((DateTime)_credential.CreatedAtUtc).ToLocalTime();             
            }
            someMaterialColor = new Helpers.SomeMaterialColor();
        }

        public async override Task InitializeAsync(object navigationData)
        {
            await base.InitializeAsync(navigationData);
            _eventAggregator.GetEventByType<ApplicationEvent>()
                .Where(_ => _.Type == ApplicationEventType.DeleteAllCredentials)
                .Subscribe(async _ => await DeleteAll());
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

        private async Task DeleteCredential()
        {
            var result = await UserDialogs.Instance.ConfirmAsync("This credential will be removed and can not undo?", "Alert");
            if (result)
            {
                var dialog = DialogService.Loading("Deleting");
                try
                {
                    var context = await _agentProvider.GetContextAsync();
                    await _credentialService.DeleteCredentialAsync(context, this._credential.Id);
                    _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialRemoved });
                    if (dialog.IsShowing)
                    {
                        dialog.Hide();
                        dialog.Dispose();
                    }
                    await NavigationService.NavigateBackAsync();
                } 
                catch(AriesFrameworkException e)
                {
                    if (dialog.IsShowing)
                    {
                        dialog.Hide();
                        dialog.Dispose();
                    }
                    DialogService.Alert("Some thing with Aries!", "Error", "OK");
                }
                catch(IndyException e)
                {
                    if (dialog.IsShowing)
                    {
                        dialog.Hide();
                        dialog.Dispose();
                    }
                    DialogService.Alert("Some thing with Indy!", "Error", "OK");

                }
                catch(Exception e)
                {
                    if (dialog.IsShowing)
                    {
                        dialog.Hide();
                        dialog.Dispose();
                    }
                    DialogService.Alert("Some thing wrong!", "Error", "OK");
                }
                
            }
        }

        /// <summary>
        /// Deletes all the credentials sharing the same connection ID.
        /// Function must be called if a connection is removed
        /// </summary>
        /// <returns></returns>
         private async Task DeleteAll()
        {
            try
            {
                var context = await _agentProvider.GetContextAsync();
                var credentialsList = await _credentialService.ListAsync(context);
                //DialogService.Loading("Removing Credentials");
                if (!credentialsList.Count.Equals(0))
                {
                    foreach (var record in credentialsList)
                    {
                        if (record.ConnectionId == RelatedConnection.Id)
                        {
                            await _credentialService.DeleteCredentialAsync(context, record.Id);
                        }
                    }
                    _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialRemoved });
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        private ConnectionRecord _relatedConnection;

        public ConnectionRecord RelatedConnection
        {
            get => _relatedConnection;
            set => this.RaiseAndSetIfChanged(ref _relatedConnection, value);
        }

        private CredentialRecord _credentialRecord;

        public CredentialRecord CredentialRecord
        {
            get => _credentialRecord;
            set
            {
                this.RaiseAndSetIfChanged(ref _credentialRecord, value);
            }
        }

        private DateTime _issuedDate = DateTime.Now;
        public DateTime IssuedDate
        {
            get => _issuedDate;
            set => this.RaiseAndSetIfChanged(ref _issuedDate, value);
        }

        private string _credId;

        public string Id
        {
            get => _credId;
            set => this.RaiseAndSetIfChanged(ref _credId, value);
        }

        private CredentialState _state;

        public CredentialState State
        {
            get => _state;
            set => this.RaiseAndSetIfChanged(ref _state, value);
        }

        private string _credentialName;
        public string CredentialName
        {
            get => _credentialName;
            set => this.RaiseAndSetIfChanged(ref _credentialName, value);
        }

        private string _credentialType;
        public string CredentialType
        {
            get => _credentialType;
            set => this.RaiseAndSetIfChanged(ref _credentialType, value);
        }

        private string _organizeImageUrl = "";
        public string OrganizeImageUrl
        {
            get => _organizeImageUrl;
            set => this.RaiseAndSetIfChanged(ref _organizeImageUrl, value);
        }

        private string _credentialSubtitle;
        public string CredentialSubtitle
        {
            get => _credentialSubtitle;
            set => this.RaiseAndSetIfChanged(ref _credentialSubtitle, value);
        }

        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set => this.RaiseAndSetIfChanged(ref _isNew, value);
        }

        private string _qRImageUrl;
        public string QRImageUrl
        {
            get => _qRImageUrl;
            set => this.RaiseAndSetIfChanged(ref _qRImageUrl, value);
        }

        private string _organizeName = "";

        public string OrganizeName
        {
            get => _organizeName;
            set
            {
                this.RaiseAndSetIfChanged(ref _organizeName, value);
                _organizeColor = someMaterialColor.GetColorFromString(_organizeName);
                if(RelatedConnection.Alias.ImageUrl == null)
                    _organizeImageUrl = $"https://ui-avatars.com/api/?name={_organizeName}&length=1&background={_organizeColor}&color=fff&size=128";
            }
        }

        private string _organizeColor = "009688";
        public string OrganizeColor 
        {
            get => _organizeColor;
            set => this.RaiseAndSetIfChanged(ref _organizeColor, value);
        }

        private IEnumerable<CredentialPreviewAttribute> _attributes;
        public IEnumerable<CredentialPreviewAttribute> Attributes
        {
            get => _attributes;
            set => this.RaiseAndSetIfChanged(ref _attributes, value);
        }

        #region Command

        public ICommand OnSelectDeleleButtonCommad =>new Command(async () => await this.DeleteCredential());

        #endregion
    }
}
