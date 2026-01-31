using System.Collections;
using MirraCloud.Example;
using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.SceneManagement;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Bootstrap
{
    public class BootstrapLoader : MonoBehaviour
    {
        private BootstrapInitializer _initializer;
        private SceneLoader _sceneLoader;
        private UIController _uiController;

        [InjectDep]
        public void Construct(BootstrapInitializer initializer, SceneLoader sceneLoader, UIController uiController)
        {
            _uiController = uiController;
            _sceneLoader = sceneLoader;
            _initializer = initializer;
        }

        private IEnumerator Start()
        {
            _uiController.ShowScreen<LoadingScreenUI>();
            
            yield return _initializer.Initialize();
            
            _sceneLoader.LoadLoginScene();
        }
    }
}