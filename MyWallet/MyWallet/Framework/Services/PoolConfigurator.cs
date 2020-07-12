using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hyperledger.Aries.Contracts;
using Hyperledger.Indy.PoolApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;

namespace MyWallet.Services
{
    public class PoolConfigurator : IHostedService
    {
        private readonly IPoolService _poolService;
        private readonly ILogger<PoolConfigurator> _logger;

        private Dictionary<string, string> poolConfigs = new Dictionary<string, string>
        {
            //{ "sovrin-live", "pool_transactions_live_genesis" },
            //{ "sovrin-builder", "pool_transactions_builder_genesis" },
            //{ "sovrin-staging", "pool_transactions_sandbox_genesis" },
            { "bcovrin-dev", "pool_transactions_bcovrin_dev_genesis" },
            { "bcovrin-test", "pool_transactions_bcovrin_test_genesis" },
            { "infyNetwork-test", "pool_transactions_infyNetwork_test_genesis" }
        };

        public PoolConfigurator(
            IPoolService poolService,
            ILogger<PoolConfigurator> logger)
        {
            _poolService = poolService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var path = Preferences.Get("ExternalDirectoryPath", FileSystem.CacheDirectory);
            string poolPath = string.Empty;
            if(!string.IsNullOrEmpty(path))
            {
               poolPath = Path.Combine(path, ".indy_client", "pool");
               if(Directory.Exists(poolPath))
                {
                    Directory.Delete(poolPath, true);
                }
            }
            foreach (var config in poolConfigs)
            {
                try
                {
                    // Path for bundled genesis txn
                    var filename = Path.Combine(FileSystem.CacheDirectory, "genesis.txn");

                    // Dump file contents to cached filename
                    using (var stream = await FileSystem.OpenAppPackageFileAsync(config.Value))
                    using (var reader = new StreamReader(stream))
                    {
                        File.WriteAllText(filename, await reader.ReadToEndAsync()
                            .ConfigureAwait(false));
                    }

                    // Create pool configuration
                    await _poolService.CreatePoolAsync(config.Key, filename)
                        .ConfigureAwait(false);
                }
                catch (PoolLedgerConfigExistsException)
                {
                    // OK
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Couldn't create pool config");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
