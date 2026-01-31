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
        builder.Register<AssetService>(Lifetime.Singleton).As<IAssetService>();
        builder.Register<PlayerProfile>(Lifetime.Singleton).AsSelf().As<ILoginInitializable>();
        builder.Register<SceneLoader>(Lifetime.Singleton);
        builder.Register<AuthService>(Lifetime.Singleton);
    }
}
