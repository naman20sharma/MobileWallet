using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Core;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace MyWallet.Framework.Services
{
    public class CloudWalletService
    {
        private static readonly SemaphoreSlim slim = new SemaphoreSlim(1, 1);
        private readonly IAgentProvider agentProvider;
        private readonly ILogger<CloudWalletService> logger;
        private readonly IEdgeClientService serviceClient;

        public CloudWalletService(
            IAgentProvider agentProvider,
            IEdgeClientService cloudServiceClient,
            ILogger<CloudWalletService> logger)
        {
            this.agentProvider = agentProvider;
            this.logger = logger;
            this.serviceClient = cloudServiceClient;
        }

        public async Task<int> FetchCloudMessagesAsync()
        {
            var messageCount = 0;
            
            try
            {
                await slim.WaitAsync();
                var inbox = await serviceClient.FetchInboxAsync(await agentProvider.GetContextAsync())
                    .ConfigureAwait(false);
                messageCount = inbox.processedCount;
                var unprocesedItems = inbox.unprocessedItems;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                logger.LogError(e, "Couldn't retrieve messages");
            }
            finally
            {
                slim.Release();
            }
            Console.WriteLine($@"{messageCount} were processed.");
            return messageCount;
        }
    }
}
