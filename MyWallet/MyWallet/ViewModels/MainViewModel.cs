using Acr.UserDialogs;
using MyWallet.Framework.Services;
using MyWallet.Messages;
using MyWallet.Services.Interfaces;
using MyWallet.ViewModels.Connections;
using MyWallet.ViewModels.Credentials;
using MyWallet.ViewModels.Notification;
using MyWallet.ViewModels.Setting;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MyWallet.ViewModels
{
    public class MainViewModel : ABaseViewModel
    {
        private CloudWalletService _cloudWalletService;

        public MainViewModel(
             IUserDialogs userDialogs,
             INavigationService navigationService,
             ConnectionsViewModel connectionsViewModel,
             CredentialsViewModel credentialsViewModel,
             NotificationViewModel notificationViewModel,
             SettingViewModel settingViewModel,
             CloudWalletService cloudWalletService
         ) : base(
                 nameof(MainViewModel),
                 userDialogs,
                 navigationService
         )
        {
            Connections = connectionsViewModel;
            Credentials = credentialsViewModel;
            Notification = notificationViewModel;
            Setting = settingViewModel;
            _cloudWalletService = cloudWalletService;

        }

        public override async Task InitializeAsync(object navigationData)
        {
            await Connections.InitializeAsync(null);
            await Credentials.InitializeAsync(null);
            await Notification.InitializeAsync(null);
            await base.InitializeAsync(navigationData);
            MessagingCenter.Send(new StartLongRunningTaskMessage(), "StartLongRunningTaskMessage");
            HandleReceivedMessages();
            Title = "Main Page";
        }

        private void HandleReceivedMessages()
        {
            System.Diagnostics.Debug.WriteLine($"Tikcked Messsage 1: work");
            MessagingCenter.Unsubscribe<TickedMessage >(this, "TickedMessage"); 

            MessagingCenter.Subscribe<TickedMessage>(this, "TickedMessage", message => {

                Device.BeginInvokeOnMainThread(async () => {
                    System.Diagnostics.Debug.WriteLine($"Tikcked Messsage 2: { message.Message}");
                    await _cloudWalletService.FetchCloudMessagesAsync();              
                });
            });

            MessagingCenter.Subscribe<CancelledMessage>(this, "CancelledMessage", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //ticker.Text = "Cancelled";
                });
            });
        }

        #region Bindable Properties
        private ConnectionsViewModel _connections;
        public ConnectionsViewModel Connections
        {
            get => _connections;
            set => this.RaiseAndSetIfChanged(ref _connections, value);
        }

        private CredentialsViewModel _credentials;
        public CredentialsViewModel Credentials
        {
            get => _credentials;
            set => this.RaiseAndSetIfChanged(ref _credentials, value);
        }

        private NotificationViewModel _notification;
        public NotificationViewModel Notification
        {
            get => _notification;
            set => this.RaiseAndSetIfChanged(ref _notification, value);
        }

        private SettingViewModel _setting;
        public SettingViewModel Setting
        {
            get => _setting;
            set => this.RaiseAndSetIfChanged(ref _setting, value);
        }
        #endregion
    }
}
