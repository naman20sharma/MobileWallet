using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using MyWallet.Events;
using MyWallet.Extensions;
using MyWallet.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MyWallet.ViewModels.Credentials
{
    public class CredentialsViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentProvider;
        private readonly ICredentialService _credentialService;
        private readonly ILifetimeScope _scope;
        private readonly IConnectionService _connectionService;
        private readonly IEventAggregator _eventAggregator;

        public CredentialsViewModel(IUserDialogs userDialogs,
                                  INavigationService navigationService,
                                  IAgentProvider agentProvider,
                                  ICredentialService credentialService,
                                  IConnectionService connectionService,
                                  IEventAggregator eventAggregator,
                                  ILifetimeScope scope) :
            base(nameof(CredentialsViewModel), userDialogs, navigationService)
        {
            _agentProvider = agentProvider;
            _credentialService = credentialService;
            _scope = scope;
            _connectionService = connectionService;
            _eventAggregator = eventAggregator;
            Title = "Credentials";
        }

        public async override Task InitializeAsync(object navigationData)
        {
            await base.InitializeAsync(navigationData);
            _eventAggregator.GetEventByType<ApplicationEvent>()
              .Where(_ => _.Type == ApplicationEventType.CredentialRemoved)
              .Subscribe(async _ => await LoadCredential());
            _eventAggregator.GetEventByType<ApplicationEvent>()
              .Where(_ => _.Type == ApplicationEventType.CredentialsUpdated)
              .Subscribe(async _ => await LoadCredential());
            _eventAggregator.GetEventByType<ApplicationEvent>()
              .Where(_ => _.Type == ApplicationEventType.ConnectionRemoved)
              .Subscribe(async _ => await DeleteAll());
            await LoadCredential();
            IsRefreshing = false;
        }

        string _test = "Worked";
        public string Test
        {
            get => _test;
            set => this.RaiseAndSetIfChanged(ref _test, value);
        }

        private async Task LoadCredential()
        {
            try
            {
                IsRefreshing = true;
                var context = await _agentProvider.GetContextAsync();
                var credentialRecordsList = await _credentialService.ListAsync(context);
                if (credentialRecordsList != null)
                {
                    IList<CredentialViewModel> credentialVms = new List<CredentialViewModel>();
                    foreach (var record in credentialRecordsList)
                    {

                        var relatedConnection = (await _connectionService.ListConnectedConnectionsAsync(context)).Any(_ => _.Id == record.ConnectionId) ? await _connectionService.GetAsync(context, record.ConnectionId) : null;
                        if (relatedConnection != null)
                        {
                            CredentialViewModel credViewModel = _scope.Resolve<CredentialViewModel>(new NamedParameter("credential", record));
                            credViewModel.RelatedConnection = relatedConnection;
                            if (credViewModel.RelatedConnection.Alias.Name != null)
                                credViewModel.OrganizeName = credViewModel.RelatedConnection.Alias.Name;
                            //credViewModel.CredentialRecord = item;
                            credentialVms.Add(credViewModel);
                        }
                    }
                    _credentialVm.Clear();
                    _credentialVm.InsertRange(credentialVms);
                }

                IsRefreshing = false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                IsRefreshing = false;
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
                var deletedConnection = Preferences.Get("DeletedConnection", null);
                if (!credentialsList.Count.Equals(0) && deletedConnection != null)
                {
                    foreach (var record in credentialsList)
                    {
                        if (record.ConnectionId == deletedConnection)
                        {
                            await _credentialService.DeleteCredentialAsync(context, record.Id);
                        }
                    }
                    _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialRemoved });
                }
                if (Preferences.ContainsKey("DeletedConnection"))
                    Preferences.Remove("DeletedConnection");
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        private RangeEnabledObservableCollection<CredentialViewModel> _credentialVm = new RangeEnabledObservableCollection<CredentialViewModel>();

        public RangeEnabledObservableCollection<CredentialViewModel> CredentialViewModels
        {
            get => _credentialVm;
            set => this.RaiseAndSetIfChanged(ref _credentialVm, value);
        }


        public ICommand RefreshCredentialsCommand => new Command(async () => await LoadCredential());

        private bool _isRefreshing = false;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => this.RaiseAndSetIfChanged(ref _isRefreshing, value);
        }

        public ICommand SelectCredentialCommand => new Command<CredentialViewModel>(async (credentialVm) =>
        {
            if (credentialVm != null)
            {
                await NavigationService.NavigateToAsync<CredentialViewModel>(credentialVm);
            }
        });

    }
}
