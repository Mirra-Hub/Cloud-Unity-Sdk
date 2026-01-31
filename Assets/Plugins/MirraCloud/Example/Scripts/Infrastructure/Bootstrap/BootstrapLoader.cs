using System.Collections;
using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.SceneManagement;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Bootstrap
{
    public class BootstrapLoader : MonoBehaviour
    {
        private BootstrapInitializer _initializer;
        private SceneLoader _sceneLoader;

        [InjectDep]
        public void Construct(BootstrapInitializer initializer, SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
            _initializer = initializer;
        }

        private IEnumerator Start()
        {
            yield return _initializer.Initialize();
            
            _sceneLoader.LoadLobby();
        }
    }
}