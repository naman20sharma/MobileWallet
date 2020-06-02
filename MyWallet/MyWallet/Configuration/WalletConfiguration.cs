namespace MyWallet.Configuration
{
    public class WalletAppConfiguration : IWalletAppConfiguration
    {
        public string AgentFrameworkEndpoint { get; set; }
        public string PoolConfigurationName { get; set; }
    }

    public interface IWalletAppConfiguration
    { 
        string AgentFrameworkEndpoint { get; set; }
        string PoolConfigurationName { get; set; }
    }
}