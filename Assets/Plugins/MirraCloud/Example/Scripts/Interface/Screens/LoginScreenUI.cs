using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoginScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _logiDevicenButton;
        [SerializeField] private Button _logiGuestButton;
        // The nickname input was previously used to seed guest accounts. The
        // server now generates guest nicknames itself, so the field is unused.
        // Kept on the prefab for backward compatibility — remove the reference
        // and the GameObject in a follow-up scene cleanup.
#pragma warning disable CS0414
        [SerializeField] private TMP_InputField _nicknameInputField;
#pragma warning restore CS0414

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
