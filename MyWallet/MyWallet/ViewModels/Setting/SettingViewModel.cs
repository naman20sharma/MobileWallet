using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Routing;
using Hyperledger.Aries.Storage;
using MyWallet.Extensions;
using MyWallet.Models.Setting;
using MyWallet.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MyWallet.ViewModels.Setting
{
    public class SettingViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentProvider;
        private readonly IEdgeClientService _edgeClientService;
        private readonly IWalletService _walletService;
        private readonly IProvisioningService _provisioningService;
        public SettingViewModel(IUserDialogs userDialogs,
            IEdgeClientService edgeClientService,
            IWalletService walletService,
            IProvisioningService provisioningService,
            IAgentProvider agentProvider,
            INavigationService navigationService) :
            base(nameof(SettingViewModel), userDialogs, navigationService)
        {
            Title = "Setting";
            _edgeClientService = edgeClientService;
            _agentProvider = agentProvider;
            //_agentOptions = options.Value;
            _provisioningService = provisioningService;
            IList<NetworkItem> networkItems = new List<NetworkItem>()
            {
                new NetworkItem("Bcovrin Test", "bcovrin-test"),
                new NetworkItem("Bcovrin Green Dev", "bcovrin-dev"),
                new NetworkItem("Sovrin Staging", "sovrin-staging"),
                new NetworkItem("Infy Network Test", "infyNetwork-test")
            };
            _networkItems.InsertRange(networkItems);
            string selectedNetwork = Preferences.Get("PoolConfigurationName", "");
            _selectedNetworkItem = networkItems.SingleOrDefault(s => s.PoolName == selectedNetwork);
        }

        public override async Task InitializeAsync(object navigationData)
        {

            await base.InitializeAsync(navigationData);
        }

        private NetworkItem _selectedNetworkItem;

        public NetworkItem SelectedNetworkItem
        {
            get => _selectedNetworkItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedNetworkItem, value);
                Preferences.Set("PoolConfigurationName", _selectedNetworkItem.PoolName);
            }
        }

        private RangeEnabledObservableCollection<NetworkItem> _networkItems = new RangeEnabledObservableCollection<NetworkItem>();

        public RangeEnabledObservableCollection<NetworkItem> NetworkItems
        {
            get => this._networkItems;
            set => this.RaiseAndSetIfChanged(ref _networkItems, value);
        }

        private async Task CreateBackup()
        {
            try
            {
                var context = await _agentProvider.GetContextAsync();
                string passPhrase = "ottava swill sniggle saint ungula bombing";
                var seed = this.ConvertStringtoMD5(passPhrase);
                string backup = await _edgeClientService.CreateBackupAsync(context, seed: seed);
                System.Diagnostics.Debug.WriteLine(backup);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Backup Error: {0}", e);
                throw;
            }
        }

        private string ConvertStringtoMD5(string strword)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(strword);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        //        private async Task CreateBackup()
        //        {
        //            var dialog = UserDialogs.Instance.Loading("Creating backup");
        //            try
        //            {
        //                var context = await _agentProvider.GetContextAsync();
        //                var wallet = context.Wallet;
        //                var externalDirectoryPath = Preferences.Get("ExternalDirectoryPath", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
        //                var backupPath = Path.Combine(externalDirectoryPath, ".Indy_Backups", "Export_Wallet");
        //                if (File.Exists(backupPath))
        //                    File.Delete(backupPath);
        //                dialog?.Show();
        //                string passPhrase = "ottava swill sniggle saint ungula bombing";
        //                string key = this.CreatePassphraseEncryption(passPhrase.Trim());
        //                var backupConfig = new WalletBackupConfig
        //                {
        //                    path = backupPath,
        //                    key = key
        //                };
        //                var exportJson = JsonConvert.SerializeObject(backupConfig, Formatting.Indented);
        //                Debug.WriteLine(exportJson);
        //                await Task.Delay(TimeSpan.FromSeconds(1));
        //                await wallet.ExportAsync(exportConfig: exportJson);
        //                dialog?.Hide();
        //                dialog?.Dispose();
        //                dialog?.TryDispose();
        //                var toastConfig = new ToastConfig("Wallet Backed up successfully!");
        //                toastConfig.BackgroundColor = Color.Green;
        //                toastConfig.Position = ToastPosition.Bottom;
        //                toastConfig.SetDuration(TimeSpan.FromSeconds(3));
        //                DialogService.Toast(toastConfig);
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.WriteLine(e);
        //                dialog?.Hide();
        //                dialog?.Dispose();
        //                dialog?.TryDispose();
        //                await DialogService.AlertAsync("unable to export wallet", title: "IO Error");
        //            }
        //        }
        //​
        //        private string CreatePassphraseEncryption(string passPhrase)
        //        {
        //            string hash = string.Empty;
        //            using (var sha256 = SHA256.Create())
        //            {
        //                Debug.WriteLine($"Hash: {passPhrase.GetHashCode()} UTF8: {passPhrase.GetUTF8Bytes()}");
        //                byte[] hashValue = sha256.ComputeHash(passPhrase.GetUTF8Bytes());
        //                hash = hashValue.GetUTF8String();
        //            }
        //            return hash;
        //        }
        //​
        //        private async Task RestoreBackup()
        //        {
        //            try
        //            {
        //                var context = await _agentProvider.GetContextAsync();
        //                string seed = "00000000000000000000000000000000";
        //                Debug.WriteLine($"Wallet is Open: {context.Wallet.IsOpen}\n Key: {_agentOptions.WalletCredentials.Key}\n" +
        //                    $"Key Derivation: {_agentOptions.WalletCredentials.KeyDerivationMethod}\n Storage Credentials:{_agentOptions.WalletCredentials.StorageCredentials}");
        //                await Task.Delay(TimeSpan.FromSeconds(1));
        //                var backups = await _edgeClientService.ListBackupsAsync(context);
        //                var attachments = await _edgeClientService.RetrieveBackupAsync(context, seed);
        //​
        //                await _edgeClientService.RestoreFromBackupAsync(context, seed);
        //​
        //                await NavigationService.NavigateToAsync<MainViewModel>();
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.WriteLine(e);
        //                await DialogService.AlertAsync($"Unable to Restore the wallet, Wallet is opened already.");
        //            }
        //        }
        //​
        //        private async Task CheckBackup()
        //        {
        //            try
        //            {
        //                var context = await _agentProvider.GetContextAsync();
        //                string seed = "00000000000000000000000000000000";
        //                Debug.WriteLine($"Wallet is Open: {context.Wallet.IsOpen}\n Key: {_agentOptions.WalletCredentials.Key}\n" +
        //                    $"Key Derivation: {_agentOptions.WalletCredentials.KeyDerivationMethod}\n Storage Credentials:{_agentOptions.WalletCredentials.StorageCredentials}");
        //                await Task.Delay(TimeSpan.FromSeconds(1));
        //                var backups = await _edgeClientService.ListBackupsAsync(context);
        //                var provisionRecord = await _provisioningService.GetProvisioningAsync(context.Wallet);
        //​
        //                var attachments = await _edgeClientService.RetrieveBackupAsync(context, seed);
        //                Debug.WriteLine($"ID: {attachments[0].Id}\n{attachments[0].Filename}");
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.WriteLine(e);
        //                throw e;
        //            }
        //        }

        public ICommand CreateBackUpCommand => new Command(async () => await this.CreateBackup());
    }
}
