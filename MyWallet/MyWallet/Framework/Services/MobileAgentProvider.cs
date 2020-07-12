using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Ledger;
using Hyperledger.Aries.Storage;
using Xamarin.Essentials;

namespace MyWallet.Framework.Services
{
    public class MobileAgentProvider : IAgentProvider
    {
        private readonly IAgent _agent;
        private readonly IPoolService _poolService;
        private readonly IWalletService _walletService;

        public MobileAgentProvider(
            IAgent agent,
            IPoolService poolService,
            IWalletService walletService)
        {
            _agent = agent;
            _poolService = poolService;
            _walletService = walletService;
        }

        public Task<IAgent> GetAgentAsync(params object[] args)
        {
            return Task.FromResult(_agent);
        }

        public async Task<IAgentContext> GetContextAsync(params object[] args)
        {
            var configurationName = Preferences.Get(Constants.PoolConfigurationName, "bcovrin-test");
            try
            {
                var agentContext = new DefaultAgentContext
                {
                    SupportedMessages = _agent.GetSupportedMessageTypes(),
                    Wallet = await _walletService.GetWalletAsync(
                   configuration: new WalletConfiguration
                   {
                       Id = Constants.LocalWalletIdKey,
                       StorageConfiguration = new WalletConfiguration.WalletStorageConfiguration
                       {
                           Path = Path.Combine(
                               FileSystem.AppDataDirectory,
                               ".indy_client",
                               "wallets")
                       }
                   },
                   credentials: new WalletCredentials
                   {
                       Key = await SyncedSecureStorage.GetAsync(Constants.LocalWalletCredentialKey)
                   }),

                   Pool = new PoolAwaitable(async () => await _poolService.GetPoolAsync(configurationName, 2)),
                   Agent = _agent
                };
                return agentContext;
            } 
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }

        }
    }
}
