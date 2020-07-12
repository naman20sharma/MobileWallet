using MyWallet.ViewModels;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyWallet.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabbedPage, IRootView
    {
        public MainPage()
        {
            InitializeComponent();
            if (Preferences.Get("IsNotified", false))
                this.SetCurrentPageAfterNotification();
            NotificationCenter.Current.NotificationTapped += OnLocalNotificationTapped;
            Preferences.Set("IsNotified", false);
            Preferences.Remove("IsNotified");
            Preferences.Remove("NotificationType");
        }

        private void CurrentPageChanged(object sender, System.EventArgs e) => Title = GetPageName(CurrentPage);

        private void Appearing(object sender, System.EventArgs e) => Title = GetPageName(CurrentPage);

        private string GetPageName(Page page)
        {
            if (page.BindingContext is ABaseViewModel vmBase)
                return vmBase.Title;
            return null;
        }

        private void OnLocalNotificationTapped(NotificationTappedEventArgs e)
        {
            Preferences.Set("IsNotified", true);
            Preferences.Set("NotificationType", e.Data);
        }
        private void SetCurrentPageAfterNotification()
        {
            var notificationData = Preferences.Get("NotificationType", null);
            if (notificationData == null)
                return;
            if (notificationData.Contains("issued"))
                this.CurrentPage = this.Children.ElementAtOrDefault(1);
            else if (notificationData.Contains("new Connection") 
                || notificationData.Contains("message"))
                this.CurrentPage = this.Children.ElementAtOrDefault(0);
            else
                this.CurrentPage = this.Children.ElementAtOrDefault(2);
        }
    }
}
