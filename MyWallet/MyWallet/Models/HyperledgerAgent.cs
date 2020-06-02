using System;
using Hyperledger.Aries.Agents;

namespace MyWallet.Models
{
    public class HyperledgerAgent : AgentBase
    {
        public HyperledgerAgent(IServiceProvider provider) : base(provider)
        {
        }

        protected override void ConfigureHandlers()
        {
            AddConnectionHandler();
            AddForwardHandler();
            AddCredentialHandler();
            AddProofHandler();
        }
    }
}