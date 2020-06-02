using Autofac;

namespace MyWallet.DependencyInjection
{
    public class ViewModelsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .Where(x => x.Namespace.Contains("MyWallet.ViewModels"))
                .InstancePerDependency();
        }
    }
}
