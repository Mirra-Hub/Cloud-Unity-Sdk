using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoginScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _logiDevicenButton;
        [SerializeField] private Button _logiGuestButton;
       
        private LoginService _loginService;

        [InjectDep]
        public void Construct(LoginService loginService)
        {
            _loginService = loginService;
        }
        
        protected override void OnEnableScreen()
        {
            _logiDevicenButton.onClick.AddListener(LoginDevice);
            _logiGuestButton.onClick.AddListener(LoginGuest);
        }

        protected override void OnDisableScreen()
        {
            _logiDevicenButton.onClick.RemoveListener(LoginDevice);
            _logiGuestButton.onClick.RemoveListener(LoginGuest);
        }

        private void LoginDevice()
        {
            _loginService.LoginDevice();
        }
        
        private void LoginGuest()
        {
            _loginService.LoginGuest();
        }
    }
}
