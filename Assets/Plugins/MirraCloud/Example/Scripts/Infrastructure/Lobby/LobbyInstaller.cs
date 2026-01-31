using MirraCloud.Example.Infrastructure.DI;
using MirraCloud.Example.Interface;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby
{
    public class LobbyInstaller : LifetimeScope
    {
        [SerializeField] private UIController _uiController;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ContainerResolver>(Lifetime.Singleton).As<IResolverDI>();
            builder.RegisterInstance<UIController>(_uiController);
            builder.Register<FactoryUI>(Lifetime.Singleton);
        }
    }
}
