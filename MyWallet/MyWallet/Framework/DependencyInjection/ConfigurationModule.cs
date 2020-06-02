using System.Reflection;
using Autofac;
using MyWallet.Configuration;
using MyWallet.Utils;
using Module = Autofac.Module;

namespace MyWallet.Framework.DependencyInjection
{
    public class ConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(
                    c => Assembly.GetAssembly(typeof(IWalletAppConfiguration)).GetEmbeddedConfiguration())
                .SingleInstance();
        }
    }
}
