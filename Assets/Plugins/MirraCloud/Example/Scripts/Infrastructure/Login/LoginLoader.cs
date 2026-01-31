using MirraCloud.Example;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby
{
    public class LoginLoader : MonoBehaviour
    {
        private UIController _uiController;

        [InjectDep]
        public void Construct(UIController uiController)
        {
            _uiController = uiController;
        }

        private void Start()
        {
            _uiController.ShowScreen<LoginScreenUI>();
        }
    }
}