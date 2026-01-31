using System;
using UnityEngine;

namespace MirraCloud.Example.Infrastructure.DI
{
    public interface IResolverDI
    {
        T Resolve<T>();
        T Instantiate<T>(T prefab, Transform parent = null) where T : MonoBehaviour;
        void InjectGameObject(GameObject gameObject);
    }
}
