using MirraCloud.Example;
using MirraCloud.Example.Infrastructure.DI;
using MirraCloud.Example.Interface;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby
{
    public class LobbyLoader : MonoBehaviour
    {
        private UIController _uiController;

        [InjectDep]
        public void Construct(UIController uiController)
        {
            _uiController = uiController;
        }

        private void Start()
        {
            _uiController.ShowScreen<LobbyScreenUI>();
        }
    }
}