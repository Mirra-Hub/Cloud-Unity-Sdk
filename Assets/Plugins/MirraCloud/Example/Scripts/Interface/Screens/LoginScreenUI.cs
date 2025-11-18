using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoginScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _logiDevicenButton;
        [SerializeField] private Button _loginWebIdButton;

        protected override void OnEnableScreen()
        {
            _logiDevicenButton.onClick.AddListener(LoginDevice);
            _loginWebIdButton.onClick.AddListener(LoginWebId);
        }

        protected override void OnDisableScreen()
        {
            _logiDevicenButton.onClick.RemoveListener(LoginDevice);
            _loginWebIdButton.onClick.RemoveListener(LoginWebId);
        }

        private async void LoginDevice()
        {
            var authOperation = MirraCloudSDK.Authentication.LoginWithDeviceIDAsync(SystemInfo.deviceUniqueIdentifier);

            await authOperation.Task;

            if (MirraCloudSDK.Authentication.IsAuth)
            {
                UIController.ShowScreen<LoadingScreenUI>();
                
                await MirraCloudSDK.RuleConstructor.LoadConfigAsync().Task;
                await MirraCloudSDK.Segments.LoadConfigAsync().Task;
            
            }
            else if (authOperation.IsError)
            {
                UIController.ShowPopup<NetworkErrorPopupUI>();
            }
        }
        
        private async void LoginWebId()
        {
            var authOperation = MirraCloudSDK.Authentication.LoginWithWebIDAsync("123456");

            await authOperation.Task;

            if (MirraCloudSDK.Authentication.IsAuth)
            {
                UIController.ShowScreen<LoadingScreenUI>();
                
                await MirraCloudSDK.RuleConstructor.LoadConfigAsync().Task;
                await MirraCloudSDK.Segments.LoadConfigAsync().Task;
            
            }
            else if (authOperation.IsError)
            {
                UIController.ShowPopup<NetworkErrorPopupUI>();
            }
        }
    }
}
