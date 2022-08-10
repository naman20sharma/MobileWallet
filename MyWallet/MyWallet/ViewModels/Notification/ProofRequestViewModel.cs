using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Indy;
using Hyperledger.Indy.AnonCredsApi;
using MyWallet.Events;
using MyWallet.Extensions;
using MyWallet.Services.Interfaces;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using System.Text.RegularExpressions;

namespace MyWallet.ViewModels.Notification
{
    public class ProofRequestViewModel : ABaseViewModel
    {
        private readonly IProofService _proofService;
        private readonly IAgentProvider _agentProvider;
        private readonly IMessageService _messageService;
        private readonly IConnectionService _connectionService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ProofRecord _proofRecord;
        public ProofRequestViewModel(IUserDialogs userDialogs,
            INavigationService navigationService, 
            IProofService proofService, 
            IAgentProvider agentProvider, 
            IMessageService messageService, 
            IEventAggregator eventAggregator,
            IConnectionService connectionService, ProofRecord proofRequestRecord) : base (nameof(ProofRequestViewModel), userDialogs, navigationService)
        {
            Title = "Proof Request";
            _proofService = proofService;
            _agentProvider = agentProvider;
            _messageService = messageService;
            _connectionService = connectionService;
            _proofRequestRecord = proofRequestRecord;
            _eventAggregator = eventAggregator;
            _proofRequest = _proofRequestRecord.RequestJson.ToObject<ProofRequest>();

            if (_proofRequestRecord.CreatedAtUtc != null)
            {
                IssuedDate = ((DateTime)_proofRequestRecord.CreatedAtUtc).ToLocalTime();
            }
        }

        public async override Task InitializeAsync(object navigationData)
        {
            IsBusy = true;
            await CreateRequestedCredential();
            await base.InitializeAsync(navigationData);
            IsBusy = false;
        }

        // private RangeEnabledObservableCollection<KeyValuePair<string, string>> _roofRequestCredentialsPairs = new RangeEnabledObservableCollection<KeyValuePair<string, string>>();

        //Dictionary<string, string> proofAndCredentialPredicatesMapping = new Dictionary<string, string>();
        public string RemoveAllSpaceAndToLower(string s)
        {
            if (s == null)
                return string.Empty;
            return Regex.Replace(s.ToLower(), @"\s+", "");
        }
        private async Task CreateRequestedCredential()
        {
            var requestedCredentials = new RequestedCredentials();
            var context = await _agentProvider.GetContextAsync();
            _proofRequestAndCredentialMaps.Clear();
            RangeEnabledObservableCollection<ProofRequestAndCredentialMap> proofRequestMapList = new RangeEnabledObservableCollection<ProofRequestAndCredentialMap>();
            try
            {
                foreach (var requestedAttribute in _proofRequest.RequestedAttributes)
                {
                    ProofRequestAndCredentialMap proofCredMap = new ProofRequestAndCredentialMap();

                    proofCredMap.ProofKey = requestedAttribute.Key;

                    var credentials = await _proofService.ListCredentialsForProofRequestAsync(context, _proofRequest,
                            requestedAttribute.Key);
                    if (credentials.Count != 0)
                    {
                        var firstSuitableCredential = credentials.First();
                        _isSatisfied = true;
                        proofCredMap.Referent = firstSuitableCredential.CredentialInfo.Referent;

                        var key = this.RemoveAllSpaceAndToLower(requestedAttribute.Value.Name);
                        var proofKeyAndCredentialMap = firstSuitableCredential.CredentialInfo.Attributes.ToDictionary(k => RemoveAllSpaceAndToLower(k.Key), v => new KeyValuePair<string, string>(v.Key, v.Value));
                        if (proofKeyAndCredentialMap.ContainsKey(key))
                        {
                            var value = proofKeyAndCredentialMap[key].Value;
                            var credKey = proofKeyAndCredentialMap[key].Key;
                            KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(credKey, value);
                            proofCredMap.CredentialAttribute = keyValuePair;
                        }

                        requestedCredentials.RequestedAttributes.Add(requestedAttribute.Key,
                            new RequestedAttribute
                            {
                                CredentialId = firstSuitableCredential.CredentialInfo.Referent,
                                Revealed = true
                            });
                    }
                    else
                    {
                        _isSatisfied = false;
                        proofCredMap.Referent = "Unavailable";
                        var key = requestedAttribute.Value.Name;
                        var value = "Unavailable";
                        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(key, value);
                        proofCredMap.CredentialAttribute = keyValuePair;

                        requestedCredentials.RequestedAttributes.Add(requestedAttribute.Key,
                            new RequestedAttribute
                            {
                                CredentialId = "Unavailable",
                                Revealed = true
                            });
                    }

                    proofRequestMapList.Add(proofCredMap);
                    //requestedCredentials.RequestedAttributes.
                    //proofAndCredentialAttributesMapping.Add(requestedAttribute, credentials.First().CredentialInfo.Attributes.)
                }

                foreach (var requestedAttribute in _proofRequest.RequestedPredicates)
                {
                    var credentials = await _proofService.ListCredentialsForProofRequestAsync(context, _proofRequest,
                           requestedAttribute.Key);
                    ProofRequestAndCredentialMap proofCredMap = new ProofRequestAndCredentialMap();
                    if (credentials.Count != 0)
                    {
                        var firstSuitableCredential = credentials.First();
                        _isSatisfied = true;
                        proofCredMap.Referent = firstSuitableCredential.CredentialInfo.Referent;

                        var key = this.RemoveAllSpaceAndToLower(requestedAttribute.Value.Name);
                        var proofKeyAndCredentialMap = firstSuitableCredential.CredentialInfo.Attributes.ToDictionary(k => RemoveAllSpaceAndToLower(k.Key), v => new KeyValuePair<string, string>(v.Key, v.Value));
                        if (proofKeyAndCredentialMap.ContainsKey(key))
                        {
                            var value = proofKeyAndCredentialMap[key].Value;
                            var credKey = proofKeyAndCredentialMap[key].Key;
                            KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(credKey, value);
                            proofCredMap.CredentialAttribute = keyValuePair;
                        }
                        requestedCredentials.RequestedPredicates.Add(requestedAttribute.Key,
                            new RequestedAttribute
                            {
                                CredentialId = firstSuitableCredential.CredentialInfo.Referent,
                                Revealed = true
                            });
                    }
                    else
                    {
                        _isSatisfied = false;
                        proofCredMap.ProofKey = requestedAttribute.Key;
                        proofCredMap.Referent = "Unavailable";
                        var key = requestedAttribute.Value.Name;
                        var value = "Unavailable";
                        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(key, value);
                        proofCredMap.CredentialAttribute = keyValuePair;
                        requestedCredentials.RequestedPredicates.Add(requestedAttribute.Key,
                            new RequestedAttribute
                            {
                                CredentialId = "Unavailable",
                                Revealed = true
                            });
                    }

                    proofRequestMapList.Add(proofCredMap);
                }
                ProofRequestAndCredentialMaps = proofRequestMapList;
                RequestedCredentials = requestedCredentials;
            }
            catch (IndyException e)
            {
                Console.WriteLine("Indy Error: " + e.Message);
                DialogService.Alert(e.Message, "Indy library error");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                DialogService.Alert("Unknow error. We will fixed it later");
            }

        }
        private async Task AcceptProofRequest()
        {
            var loadingDialog = DialogService.Loading("Proccessing");
            try
            {

                this.IsBusy = true;
                var context = await _agentProvider.GetContextAsync();
                var (message, proofRecord) = await _proofService.CreatePresentationAsync(context, ProofRequestRecord.Id, RequestedCredentials);
                var connectionRecord = await _connectionService.GetAsync(context, proofRecord.ConnectionId);
                await _messageService.SendAsync(context, message, connectionRecord);
                loadingDialog.Hide();
                this.IsBusy = false;
                await NavigationService.NavigateBackAsync();
                var toastConfig = new ToastConfig("Accepted Proof!");
                toastConfig.BackgroundColor = Color.Green;
                toastConfig.Position = ToastPosition.Bottom;
                toastConfig.SetDuration(3000);
                DialogService.Toast(toastConfig);
                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.GotProofRequestMessage });
            }
            catch (IndyException e)
            {
                this.IsBusy = false;
                loadingDialog.Hide();
                if (e.SdkErrorCode == 212)
                    DialogService.Alert("You don't have any suitable credential to present", "Error", "OK");
                else
                {
                    Console.WriteLine("Indy Error: " + e.Message);
                    DialogService.Alert("Some error with libindy. We're working on it", "Error", "OK");
                }

            }
            catch (Exception e)
            {
                this.IsBusy = false;
                loadingDialog.Hide();
                Console.WriteLine("Error: " + e.Message);
                DialogService.Alert("Error while accept Proof Request");
            }

        }

        private async Task RejectProofRequest()
        {
            var loadingDialog = DialogService.Loading("Proccessing");
            try
            {

                this.IsBusy = true;
                var context = await _agentProvider.GetContextAsync();
                // var (message, proofRecord) = await _proofService.CreatePresentationAsync(context, ProofRequestRecord.Id, RequestedCredentials);
                await _proofService.RejectProofRequestAsync(context, ProofRequestRecord.Id);
                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialsUpdated });
                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.NotificationUpdated });
                loadingDialog.Hide();
                await NavigationService.NavigateBackAsync();
                this.IsBusy = false;
                var toastConfig = new ToastConfig("Rejected successfully!");
                toastConfig.BackgroundColor = Color.Green;
                toastConfig.Position = ToastPosition.Bottom;
                toastConfig.SetDuration(3000);
                DialogService.Toast(toastConfig);
                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.GotProofRequestMessage });
            }
            catch (Exception e)
            {
                this.IsBusy = false;
                loadingDialog.Hide();
                DialogService.Alert("Error while Reject Proof Request");
            }

        }


        #region Bindable Props 
        private ProofRecord _proofRequestRecord;

        public ProofRecord ProofRequestRecord
        {
            get => _proofRequestRecord;
            set
            {
                this.RaiseAndSetIfChanged(ref _proofRequestRecord, value);
            }
        }

        private ProofRequest _proofRequest;
        public ProofRequest ProofRequestObject
        {
            get => this._proofRequest;
            set => this.RaiseAndSetIfChanged(ref _proofRequest, value);
        }

        private RequestedCredentials _requestedCredentials;
        public RequestedCredentials RequestedCredentials
        {
            get => this._requestedCredentials;
            set => this.RaiseAndSetIfChanged(ref _requestedCredentials, value);
        }

        private RangeEnabledObservableCollection<ProofRequestAndCredentialMap> _proofRequestAndCredentialMaps = new RangeEnabledObservableCollection<ProofRequestAndCredentialMap>();
        public RangeEnabledObservableCollection<ProofRequestAndCredentialMap> ProofRequestAndCredentialMaps
        {
            get => _proofRequestAndCredentialMaps;
            set => this.RaiseAndSetIfChanged(ref _proofRequestAndCredentialMaps, value);
        }

        private DateTime _issuedDate = DateTime.Now;
        public DateTime IssuedDate
        {
            get => _issuedDate;
            set => this.RaiseAndSetIfChanged(ref _issuedDate, value);
        }

        private bool _isSatisfied = false;
        public bool IsSatisfied
        {
            get => _isSatisfied;
            set 
            { 
                this.RaiseAndSetIfChanged(ref _isSatisfied, value);
            }
        }
        #endregion

        #region Bindable Command 
        public ICommand AcceptProofRequestCommand => new Command(async () => await AcceptProofRequest());
        public ICommand RejectProofRequestCommand => new Command(async () => await RejectProofRequest());
        #endregion
    }
}
