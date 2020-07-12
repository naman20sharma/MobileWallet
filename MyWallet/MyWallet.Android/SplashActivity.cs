
using Android.App;
using Android.Content;
using Android.OS;

namespace MyWallet.Droid
{
    [Activity(Theme = "@style/Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        private static readonly string ExtraReturnDataAndroid = "Plugin.LocalNotification.RETURN_DATA";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            var data = Intent.GetStringExtra(ExtraReturnDataAndroid);
            var intent = new Intent(Application.Context, typeof(MainActivity));
            if(!string.IsNullOrWhiteSpace(data))
            {
                intent.PutExtra(ExtraReturnDataAndroid, data);
            }
            StartActivity(intent);
            //Finish();
        }
    }
}