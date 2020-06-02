using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Hyperledger.Aries.Features.DidExchange;
using MyWallet.Services.Interfaces;
using ReactiveUI;
using Xamarin.Forms;

namespace MyWallet.ViewModels.Notification
{
    public class NotificationItem : ReactiveObject
    {

        public NotificationItem()
        {

        }

        #region Bindable props
        public ConnectionAlias _organizeAlias = new ConnectionAlias { Name="Unknow Org", ImageUrl = "" };

        public ConnectionAlias OrganizeAlias
        {
            get => _organizeAlias;
            set => this.RaiseAndSetIfChanged(ref _organizeAlias, value);
        }

        private string _notificationTitle = "Notification";
        public string NotificationTitle
        {
            get => _notificationTitle;
            set => this.RaiseAndSetIfChanged(ref _notificationTitle, value);
        }

        private string _notificationType = "System";
        public string NotificationType
        {
            get => _notificationType;
            set => this.RaiseAndSetIfChanged(ref _notificationType, value);
        }

        private string _notificationContent = "You've got a notification";

        public string NotificationContent
        {
            get => _notificationContent;
            set => this.RaiseAndSetIfChanged(ref _notificationContent, value);
        }

        private ABaseViewModel _itemViewModel;

        public ABaseViewModel ItemViewModel
        {
            get => _itemViewModel;
            set => this.RaiseAndSetIfChanged(ref _itemViewModel, value);
        }

        private DateTime _issuedDate = DateTime.Now;
        public DateTime IssuedDate
        {
            get => _issuedDate;
            set => this.RaiseAndSetIfChanged(ref _issuedDate, value);
        }
        #endregion
    }
}
