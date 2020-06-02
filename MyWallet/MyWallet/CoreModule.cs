using Acr.UserDialogs;
using Autofac;
using MyWallet.Services;
using Microsoft.Extensions.Logging;
using Xamarin.Forms;

namespace MyWallet
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder
                .Register(x => UserDialogs.Instance)
                .As<IUserDialogs>()
                .SingleInstance();

            builder
                .RegisterType<NavigationService>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder
                .Register(_ => new LoggerFactory())
                .As<ILoggerFactory>()
                .SingleInstance();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            //builder
            //    .RegisterAssemblyTypes(ThisAssembly)
            //    .Where(x => x.Namespace.Contains("Osma.Mobile.App.ViewModels"))
            //    .InstancePerDependency();


            //builder
            //    .RegisterAssemblyTypes(ThisAssembly)
            //    .Where(x => x.Namespace.Contains("Osma.Mobile.App.Views"))
            //    .InstancePerDependency();


            ////builder.RegisterAssemblyTypes(typeof(ABaseViewModel).Assembly)
            ////            .Where(x => x.IsAssignableTo<ABaseViewModel>())
            ////            .AsSelf();

            //builder.RegisterAssemblyTypes(GetType().Assembly)
            //            .Where(x => x.IsAssignableTo<Page>())
            //            .AsSelf();
        }
    }
}
