using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Content;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Java.Lang;
using Acr.UserDialogs;

using Android.Views;
using Android.Widget;
using Android;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using MyWallet.Messages;
using MyWallet.Droid.Services;
using Plugin.LocalNotification;
using FFImageLoading.Forms.Platform;
using Plugin.Fingerprint;
using Xamarin.Essentials;
using Android.Webkit;
using AndroidHUD;
using Android.Systems;
using Plugin.PushNotification;
using Android.Bluetooth;
using Xamarin.Forms.Internals;
using Android.Provider;
using Hyperledger.Aries.Extensions;

namespace MyWallet.Droid
{
    [Activity(Label = "MyWallet", Icon = "@mipmap/icon", Theme = "@style/MainTheme",
        MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private App _application;
        public static Context Context;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            //for indicator view 
            //Xamarin.Forms.Forms.SetFlags(new string[] { "IndicatorView_Experimental" });

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            CrossFingerprint.SetCurrentActivityResolver(() => this);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            //Initializing User Dialogs
            UserDialogs.Init(this);


            // Initializing FFImageLoading
            CachedImageRenderer.Init(false);


            //NotificationCenter.CreateNotificationChannel(new Plugin.LocalNotification.Platform.Droid.NotificationChannelRequest()
            //{
            //    Sound = Resource.Raw.swiftly.ToString(),
            //    VibrationPattern = new long[] {0, 1000},
            //    LockscreenVisibility = NotificationVisibility.Private
            //}) ;
            //setupPushNotification();
            //thinh nnd
            
            if ((int)Build.VERSION.SdkInt >= 23)
                CheckAndRequestRequiredPermissions();

            var host = App.BuildHost(typeof(DependencyInjection.DroidServiceModule).Assembly)
              .UseContentRoot(System.Environment.GetFolderPath(
                  System.Environment.SpecialFolder.Personal)).Build();
            
            if (Xamarin.Essentials.DeviceInfo.DeviceType.Equals(DeviceType.Physical)
                && !Preferences.ContainsKey("AgentName"))
            {
                using (var device = BluetoothAdapter.DefaultAdapter)
                {
                    if (device != null)
                    {
                        var deviceName = device.Name;
                        int digitCount = 0;
                        deviceName.ForEach(ch =>
                        {
                            if (char.IsDigit(ch))

                                digitCount++;
                        });
                        if (digitCount >= 4 && !deviceName.Contains(" "))
                            deviceName = Android.OS.Build.Device;
                        Preferences.Set("AgentName", deviceName);
                    }
                }
            }
            Preferences.Set("ExternalDirectoryPath", Android.OS.Environment.ExternalStorageDirectory.ToString());

            Os.Setenv("EXTERNAL_STORAGE", FileSystem.AppDataDirectory, true);

            try
            {
                Console.WriteLine("CRASH_TEST - loading c++_shared");
                JavaSystem.LoadLibrary("c++_shared");
            }
            catch (Java.Lang.UnsatisfiedLinkError e)
            {
                Console.WriteLine("CRASH_TEST - " + e.Message);
                Console.WriteLine("CRASH_TEST - indy_shared");
                JavaSystem.LoadLibrary("indy_shared");
            }
            Console.WriteLine("CRASH_TEST - indy");
            JavaSystem.LoadLibrary("indy");
            Console.WriteLine("CRASH_TEST - indy-loaded");

            _application = host.Services.GetRequiredService<App>();
            WireUpLongRunningTask();

            LoadApplication(_application);
            PushNotificationManager.ProcessIntent(this, Intent);
            //NotificationCenter.NotifyNotificationTapped(Intent);

        }

        private void setupPushNotification()
        {
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                //Change for your default notification channel id here
                PushNotificationManager.DefaultNotificationChannelId = "DefaultChannel";

                //Change for your default notification channel name here
                PushNotificationManager.DefaultNotificationChannelName = "General";
                PushNotificationManager.DefaultNotificationChannelImportance = NotificationImportance.Max;
            }

            //If debug you should reset the token each time.
#if DEBUG
            PushNotificationManager.Initialize(this, true);
#else
            PushNotificationManager.Initialize(this, false);
#endif

            PushNotificationManager.IconResource = Resource.Drawable.AppNotification;
        }

        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}

        readonly string[] _permissionsRequired =
        {
            Manifest.Permission.Camera
        };

        private int _requestCode = -1;
        private List<string> _permissionsToBeGranted = new List<string>();

        private void CheckAndRequestRequiredPermissions()
        {
            for (int i = 0; i < _permissionsRequired.Length; i++)
                if (CheckSelfPermission(_permissionsRequired[i]) != (int)Permission.Granted)
                    _permissionsToBeGranted.Add(_permissionsRequired[i]);

            if (_permissionsToBeGranted.Any())
            {
                _requestCode = 10;
                RequestPermissions(_permissionsRequired.ToArray(), _requestCode);
            }
            else
                System.Diagnostics.Debug.WriteLine("Device already has all the required permissions");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            if (grantResults.Length == _permissionsToBeGranted.Count)
                System.Diagnostics.Debug.WriteLine("All permissions required that werent granted, have now been granted");
            else
                System.Diagnostics.Debug.WriteLine("Some permissions requested were denied by the user");

            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            PushNotificationManager.ProcessIntent(this, intent);
            NotificationCenter.NotifyNotificationTapped(intent);
        }

        void WireUpLongRunningTask()
        {
            MessagingCenter.Subscribe<StartLongRunningTaskMessage>(this, "StartLongRunningTaskMessage", message =>
            {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StartService(intent);
            });

            MessagingCenter.Subscribe<StopLongRunningTaskMessage>(this, "StopLongRunningTaskMessage", message =>
            {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StopService(intent);
            });
        }

    }
}