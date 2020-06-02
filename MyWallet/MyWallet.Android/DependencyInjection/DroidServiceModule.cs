using Autofac;
using MyWallet.Droid.Services;

namespace MyWallet.Droid.DependencyInjection
{
    public class DroidServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ZXingHelper>().AsImplementedInterfaces().SingleInstance();
            base.Load(builder);
        }
    }
}

