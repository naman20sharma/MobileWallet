using System;
using System.Diagnostics;
using Xamarin.Forms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using System.Reflection;
using MyWallet.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
//using Microsoft.AppCenter.Push;
using MyWallet.Framework.Services;
using Xamarin.Essentials;
using MyWallet.Services;
using Hyperledger.Aries.Storage;
using System.IO;
using MyWallet.Configuration;
using MyWallet.Views;
using MyWallet.ViewModels;
using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using System.Threading.Tasks;
using MyWallet.Messages;
using Plugin.LocalNotification;
using MyWallet.Services.Middlewares;
using MyWallet.Services.Interfaces;
using MyWallet.ViewModels.Connections;
using MyWallet.Views.Connections;
using MyWallet.ViewModels.Notification;
using MyWallet.Views.Notification;
using MyWallet.ViewModels.Credentials;
using MyWallet.Views.Credentials;
using MyWallet.ViewModels.Setting;
using MyWallet.Views.Setting;
using MyWallet.Models.Onboarding;
using MyWallet.Views.Onboarding;
using MyWallet.ViewModels.Onboarding;
using Plugin.PushNotification;
using Hyperledger.Aries.Routing;
using System.Timers;

namespace MyWallet
{
    public partial class App : Application
    {
        public static IContainer Container { get; set; }
        public static IServiceProvider Provider { get; set; }
        public App()
        {
            InitializeComponent();
            timer = new Timer
            {
                Enabled = false,
                AutoReset = true,
                Interval = TimeSpan.FromSeconds(8).TotalMilliseconds
            };
            timer.Elapsed += Timer_Elapsed;
        }

        public App(IHost host) : this() => Host = host;
        private readonly Timer timer;
        public static IHost Host { get; private set; }

        public static IHostBuilder BuildHost(Assembly platformSpecific = null) =>
            XamarinHost.CreateDefaultBuilder<App>()
                .ConfigureServices((context, services) =>
                {
                    // NotificationCenter.Current.NotificationTapped += OnNotificationTapped;
                    services.AddAriesFramework();
                    services.AddAriesFramework(builder => builder.RegisterEdgeAgent(
                    options: options =>
                    {
                        var config = Container.Resolve<IWalletAppConfiguration>();
                        options.EndpointUri = config.AgentFrameworkEndpoint;

                        options.WalletConfiguration.StorageConfiguration = new WalletConfiguration.WalletStorageConfiguration
                        {
                            Path = Path.Combine(
                                path1: FileSystem.AppDataDirectory,
                                path2: ".indy_client",
                                path3: "wallets")


                        };
                        options.BackupDirectory = Path.Combine(
                                path1: FileSystem.AppDataDirectory,
                                path2: ".indy_client",
                                path3: "backups");
                        options.AgentName = Preferences.Get("AgentName", "Default");
                        options.WalletConfiguration.Id = Preferences.Get("WalletConfigurationId", Constants.LocalWalletIdKey) ;
                        options.WalletCredentials.Key = Preferences.Get("WalletCredentialId", "Default");
                        options.PoolName = config.PoolConfigurationName;
                        options.ProtocolVersion = 2;
                        options.AutoRespondCredentialOffer = true;
                        options.AutoRespondCredentialRequest = true;
                    },
                    delayProvisioning: true));


                    services.AddHostedService<PoolConfigurator>();
                    //services.OverrideDefaultAgentProvider<MobileAgentProvider>();
                    var containerBuilder = new ContainerBuilder();
                    containerBuilder.RegisterAssemblyModules(typeof(ViewModelsModule).Assembly);
                    containerBuilder.RegisterModule(new CoreModule());
                    if (platformSpecific != null)
                    {
                        containerBuilder.RegisterAssemblyModules(platformSpecific);
                    }
                    containerBuilder.Populate(services);
                    //Container.Resolve<TestMiddleWare>();
                    Container = containerBuilder.Build();

                    //Container.Resolve<MessageHandleMiddleWare>();
                    _navigationService = Container.Resolve<INavigationService>();

                });

        private static void OnNotificationTapped(NotificationTappedEventArgs e)
        {

        }

        Task InitializeTask;
        static INavigationService _navigationService;
        private async Task Initialize()
        {
            this._cloudWalletService = Container.Resolve<CloudWalletService>();
            _navigationService.AddPageViewModelBinding<ConnectionsViewModel, ConnectionsPage>();
            _navigationService.AddPageViewModelBinding<ConnectionViewModel, ConnectionPage>();
            _navigationService.AddPageViewModelBinding<NotificationViewModel, NotificationPage>();
            _navigationService.AddPageViewModelBinding<CredentialsViewModel, CredentialsPage>();
            _navigationService.AddPageViewModelBinding<CredentialViewModel, CredentialPage>();
            _navigationService.AddPageViewModelBinding<CredOfferViewModel, CredOfferPage>();
            _navigationService.AddPageViewModelBinding<ProofRequestViewModel, ProofRequestPage>();
            _navigationService.AddPageViewModelBinding<ScanCodeViewModel, ScanCodePage>();
            _navigationService.AddPopupViewModelBinding<AcceptInvitationViewModel, AcceptInvitationPopup>();
            _navigationService.AddPageViewModelBinding<MainViewModel, MainPage>();
            _navigationService.AddPageViewModelBinding<RegisterPageViewModel, RegisterPage>();
            _navigationService.AddPageViewModelBinding<SettingViewModel, SettingPage>();
            _navigationService.AddPageViewModelBinding<OnboardingViewModel, OnboardingPageV2>();
            if (Preferences.Get("LocalWalletProvisioned", false))
            {
                //Task.Run(async () => await _navigationService.NavigateToAsync<MainViewModel>());
                SetupPushNotification();
                await _navigationService.NavigateToAsync<MainViewModel>();

            }
            else
            {
                //Task.Run(async () => await _navigationService.NavigateToAsync<RegisterPageViewModelV2>());
                //Task.Run(async () => await _navigationService.NavigateToAsync<RegisterPageViewModelV2>());
                //await _navigationService.NavigateToAsync<RegisterPageViewModel>();
                await _navigationService.NavigateToAsync<OnboardingViewModel>();
                MessagingCenter.Subscribe<OnboardingViewModel>(this, "SetupPushNotification", obj => this.SetupPushNotification());
            }
            timer.Start();
        }
        protected override void OnStart()
        {
            if (Host != null)
            {
                Host.Start();
                InitializeTask = Initialize();
            }
        }

        private void SetupPushNotification()
        {

            CrossPushNotification.Current.OnTokenRefresh += async (s, p) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"TOKEN REC: {p.Token}");

                    AddDeviceInfoMessage deviceInfoMessage = new AddDeviceInfoMessage()
                    {
                        DeviceId = p.Token,
                        DeviceVendor = DeviceInfo.Platform.ToString()
                    };

                    await Container.Resolve<IEdgeClientService>().AddDeviceAsync((await Container.Resolve<IAgentProvider>().GetContextAsync()), deviceInfoMessage);

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.InnerException.Message);
                }
            };

            CrossPushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Received");
                    if (p.Data.ContainsKey("body"))
                    {
                        Debug.WriteLine($"Body: {p.Data["body"]}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            };

            CrossPushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Opened");
                foreach (var data in p.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                }

                if (!string.IsNullOrEmpty(p.Identifier))
                {
                    Debug.WriteLine($"Identifier: {p.Identifier}");
                }
                else if (p.Data.ContainsKey("color"))
                {
                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                    //    //mPage.Navigation.PushAsync(new ContentPage()
                    //    ////{
                    //        BackgroundColor = Color.FromHex($"{p.Data["color"]}")
                    //    });
                    //});
                }
                else if (p.Data.ContainsKey("aps.alert.title"))
                {
                    Debug.WriteLine($"Title: {p.Data["aps.alert.title"]}");
                }
            };
            CrossPushNotification.Current.OnNotificationDeleted += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Dismissed");
            };
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check for new messages with the mediator agent if successfully provisioned
            if (Preferences.Get("LocalWalletProvisioned", false))
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        //var context = await Container.Resolve<IAgentProvider>().GetContextAsync();
                        int count = await this._cloudWalletService.FetchCloudMessagesAsync();
                        Console.WriteLine($@"{count} were processed.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });
            }
        }
        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            // Deeplink functionality code here
        }

        protected override void OnSleep()
        {
            Host.Sleep();
            timer.Stop();
        }

        protected override void OnResume()
        {
            Host.Resume();
            timer.Start();
        }

        public CloudWalletService _cloudWalletService;



        //public async void HandlePushNotification(PushNotificationReceivedEventArgs e)
        //{
        //    Debug.WriteLine($"Push Message: {e.Title} - {e.Message}");

        //    try
        //    {
        //        var count = await Container.Resolve<CloudWalletService>().FetchCloudMessagesAsync();
        //        Debug.WriteLine($"Processed {count} cloud messages");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }
        //}

        public void OpenUrl(Uri uri)
        {
            OnAppLinkRequestReceived(uri);
        }
    }
}
