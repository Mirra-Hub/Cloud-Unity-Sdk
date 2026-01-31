using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MirraCloud.Example.Infrastructure.DI
{
    public class ContainerResolver : IResolverDI
    {
        private readonly IObjectResolver  _container;

        public ContainerResolver(IObjectResolver  container)
        {
            _container = container;
        }
        
        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public T Instantiate<T>(T prefab, Transform parent = null) where T : MonoBehaviour
        {
            return _container.Instantiate<T>(prefab, parent);
        }

        public void InjectGameObject(GameObject gameObject)
        {
            _container.InjectGameObject(gameObject);
        }

    }
}
