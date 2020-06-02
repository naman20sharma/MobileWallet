using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Routing;
using Microsoft.Extensions.Logging;

namespace MyWallet.Framework.Services
{
    public class CloudWalletService
    {
        public CloudWalletService(
            IAgentProvider agentProvider,
            IEdgeClientService cloudServiceClient,
            ILogger<CloudWalletService> logger)
        {
            this.agentProvider = agentProvider;
            this.logger = logger;
            this.serviceClient = cloudServiceClient;
        }

        private static readonly SemaphoreSlim slim = new SemaphoreSlim(1, 1);
        private readonly IAgentProvider agentProvider;
        private readonly ILogger<CloudWalletService> logger;
        private readonly IEdgeClientService serviceClient;

        public async Task<int> FetchCloudMessagesAsync()
        {
            var messageCount = 0;
            try
            {
                await slim.WaitAsync();
                await serviceClient.FetchInboxAsync(await agentProvider.GetContextAsync())
                    .ConfigureAwait(false);
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
