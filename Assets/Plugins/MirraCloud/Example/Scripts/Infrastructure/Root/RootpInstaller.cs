using MirraCloud.Core;
using MirraCloud.Example.AssetManagement.Assets;
using Plugins.MirraCloud.Example.Scripts;
using Plugins.MirraCloud.Example.Scripts.Core;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.SceneManagement;
using VContainer;
using VContainer.Unity;

public class RootpInstaller : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<MirraCloudSDK>(Lifetime.Singleton).As<IMirraCloudSdk>();

        builder.Register<AssetService>(Lifetime.Singleton).As<IAssetService>();
        builder.Register<SceneLoader>(Lifetime.Singleton);
        builder.Register<AuthService>(Lifetime.Singleton);

        builder.Register<PlayerProfile>(Lifetime.Singleton).AsSelf().As<ILoginInitializable>();
        builder.Register<EconomyService>(Lifetime.Singleton).AsSelf().As<ILoginInitializable>();
        builder.Register<PlayerEconomy>(Lifetime.Singleton).AsSelf().As<ILoginInitializable>();
    }
}
