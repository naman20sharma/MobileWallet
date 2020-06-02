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
            Xamarin.Forms.Forms.SetFlags(new string[] { "IndicatorView_Experimental" });

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            CrossFingerprint.SetCurrentActivityResolver(() => this);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            //Initializing User Dialogs
            UserDialogs.Init(this);


            // Initializing FFImageLoading
            CachedImageRenderer.Init(false);

            

            //thinh nnd
            if ((int)Build.VERSION.SdkInt >= 23)
                CheckAndRequestRequiredPermissions();

            var host = App.BuildHost(typeof(DependencyInjection.DroidServiceModule).Assembly)
              .UseContentRoot(System.Environment.GetFolderPath(
                  System.Environment.SpecialFolder.Personal)).Build();

            try
            {
                Console.WriteLine("CRASH_TEST - loading c++_shared");
                JavaSystem.LoadLibrary("c++_shared");
            }
            catch (Java.Lang.UnsatisfiedLinkError e)
            {
                Console.WriteLine("CRASH_TEST - " + e.Message);
                Console.WriteLine("CRASH_TEST - lgnustl_shared");
                JavaSystem.LoadLibrary("gnustl_shared");
            }
            Console.WriteLine("CRASH_TEST - indy");
            JavaSystem.LoadLibrary("indy");
            Console.WriteLine("CRASH_TEST - indy-loaded");

            _application = host.Services.GetRequiredService<App>();
            WireUpLongRunningTask();

            LoadApplication(_application);
            NotificationCenter.NotifyNotificationTapped(Intent);

        }
        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}

        readonly string[] _permissionsRequired =
    {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
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
            NotificationCenter.NotifyNotificationTapped(intent);
            base.OnNewIntent(intent);
        }

        void WireUpLongRunningTask()
        {
            MessagingCenter.Subscribe<StartLongRunningTaskMessage>(this, "StartLongRunningTaskMessage", message => {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StartService(intent);
            });

            MessagingCenter.Subscribe<StopLongRunningTaskMessage>(this, "StopLongRunningTaskMessage", message => {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StopService(intent);
            });
        }

    }
}