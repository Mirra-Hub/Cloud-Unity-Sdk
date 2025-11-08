using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoginScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _loginButton;

        protected override void OnEnableScreen()
        {
            _loginButton.onClick.AddListener(Login);
        }

        protected override void OnDisableScreen()
        {
            _loginButton.onClick.RemoveListener(Login);
        }

        public async void Login()
        {
            var authOperation = MirraCloudSDK.Authentication.LoginWithDeviceIDAsync(SystemInfo.deviceUniqueIdentifier);

            await authOperation.Task;

            await MirraCloudSDK.RuleConstructor.LoadConfigAsync().Task;
            await MirraCloudSDK.Segments.LoadConfigAsync().Task;

            if (MirraCloudSDK.Authentication.IsAuth)
            {
                UIController.ShowScreen<LoadingScreenUI>();
            }
            else if (authOperation.IsError)
            {
                UIController.ShowPopup<NetworkErrorPopupUI>();
            }
            
            
        }
    }
}
