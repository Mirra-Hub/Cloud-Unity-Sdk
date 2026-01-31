using VContainer;
using VContainer.Unity;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Bootstrap
{
    public class BootstrapInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<BootstrapInitializer>(Lifetime.Singleton);
        }
    }
}