using Autofac;
using Hyperledger.Aries.Agents;
using MyWallet.Framework.Services;

namespace MyWallet.DependencyInjection
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<MobileAgentProvider>()
                .AsImplementedInterfaces()
                .SingleInstance();
        
            builder.RegisterType<CloudWalletService>()
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterAssemblyTypes(typeof(ServicesModule).Assembly)
                .Where(x => typeof(IAgentMiddleware).IsAssignableFrom(x))
                .AsImplementedInterfaces()
                .SingleInstance();

        }
    }
}
