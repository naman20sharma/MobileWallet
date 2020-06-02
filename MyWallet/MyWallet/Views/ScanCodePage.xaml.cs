using Autofac;
using MyWallet.Services;
using MyWallet.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;
using ZXing.Mobile;

namespace MyWallet.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanCodePage : ContentPage
    {
       
        public ScanCodePage()
        {
            InitializeComponent();
            //ViewModel = App.Container.Resolve<ScanCodeViewModelV2>();

            ScannerView.Options = new MobileBarcodeScanningOptions
            {
                UseNativeScanning = true,
                TryHarder = true,
                AutoRotate = true,
                DisableAutofocus = false,
                PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.QR_CODE
                },
            };

            if (DeviceInfo.Platform == DevicePlatform.Android)
                ScannerView.Options.CameraResolutionSelector =
                    App.Container.Resolve<IZXingHelper>().CameraResolutionSelectorDelegateImplementation;
        }

    }
}