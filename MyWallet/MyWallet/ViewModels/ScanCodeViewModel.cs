using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Features.DidExchange;
using MyWallet.Framework.Utilities;
using MyWallet.Services.Interfaces;
using MyWallet.ViewModels.Connections;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MyWallet.ViewModels
{
    public class ScanCodeViewModel : ABaseViewModel
    {
       private readonly ILifetimeScope _scope;
        public ScanCodeViewModel(IUserDialogs userDialogs,
            INavigationService navigationService,
            ILifetimeScope scope) : 
            base(nameof(ScanCodeViewModel), userDialogs, navigationService)
        {
            _scope = scope;
            Title = "Scan QR";
        }


        public ZXing.Result Result { get; set; }

        private bool _isScanning = true;

        public bool IsScanning
        {
            get => _isScanning;
            set => this.RaiseAndSetIfChanged(ref _isScanning, value);
        }

        private bool _isAnalyzing = true;
        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set => this.RaiseAndSetIfChanged(ref _isAnalyzing, value);
        }

        public async Task ProcessCode()
        {
            string scannedCode = Result.Text;
            //bool comingFromExternalSource = false;
            try
            {
                var message = await MessageDecorder.ParseMessageAsync(scannedCode);
                Console.WriteLine($@"Decoded message {message}");
                await NavigationService.NavigateBackAsync();
                AcceptInvitationViewModel acceptInvitationViewModel = _scope.Resolve<AcceptInvitationViewModel>();
                acceptInvitationViewModel.InvitationMessage = (ConnectionInvitationMessage)message;
                //await NavigationService.NavigateBackAsync();
                await NavigationService.PopModalAsync();
                await NavigationService.NavigateToPopupAsync<AcceptInvitationViewModel>(true, acceptInvitationViewModel);
                //await Application.Current.MainPage.DisplayAlert("Scanned code", scannedCode, "Close");
                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                //await NavigationService.NavigateBackAsync();
                await NavigationService.PopModalAsync();
                await Application.Current.MainPage.DisplayAlert("Error", "Invalid QR Code", "Close");
            }
        }

        public ICommand QRScanResultCommand => new Command(() => {
                
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (IsScanning)
                    {
                        IsAnalyzing = false;
                        IsScanning = false;
                        await this.ProcessCode();
                    }
                    //do your job here - Result.Text contains QR CODE
                    IsAnalyzing = true;
                    IsScanning = true;
                });
              
        });

        public ICommand NavigationBackCommand => new Command(async () =>
        {
            await NavigationService.PopModalAsync();
            //await NavigationService.NavigateBackAsync();
        });

    }
}
