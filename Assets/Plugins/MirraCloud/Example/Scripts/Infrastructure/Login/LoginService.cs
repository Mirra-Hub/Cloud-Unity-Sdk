using System.Collections.Generic;
using System.Threading.Tasks;
using Plugins.MirraCloud.Example.Scripts.Core;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.SceneManagement;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby
{
    public class LoginService
    {
        private readonly UIController _uiController;
        private readonly AuthService _authService;
        private readonly SceneLoader _sceneLoader;
        private IEnumerable<ILoginInitializable> _initializables;

        public LoginService(AuthService authService, SceneLoader sceneLoader, UIController uiController, IEnumerable<ILoginInitializable>  initializables)
        {
            _initializables = initializables;
            _sceneLoader = sceneLoader;
            _authService = authService;
            _uiController = uiController;
        }
        
        public async void LoginGuest()
        {
            bool isSuccess = await _authService.LoginGuest();
            
            HandleLogin(isSuccess);
        }

        public async void LoginDevice()
        {
            bool isSuccess = await _authService.LoginDevice();
            
            HandleLogin(isSuccess);
        }
        
        private async void HandleLogin(bool isSuccess)
        {
            if (isSuccess)
            {
                bool result = await LoginInitialize();

                if (result == false)
                {
                    _uiController.ShowPopup<NetworkErrorPopupUI>();
                    return;
                }
                
                _sceneLoader.LoadLobbyScene();
            }
            else
            {
                _uiController.ShowPopup<NetworkErrorPopupUI>();
            }
        }

        private async Task<bool> LoginInitialize()
        {
            foreach (var initializable in _initializables)
            {
                bool result =  await initializable.Initialize();

                if (result == false)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}