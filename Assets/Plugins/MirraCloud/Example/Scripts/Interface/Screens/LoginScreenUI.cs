using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoginScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _logiDevicenButton;
        [SerializeField] private Button _logiDevicen2Button;

        protected override void OnEnableScreen()
        {
            _logiDevicenButton.onClick.AddListener(LoginDevice);
            _logiDevicen2Button.onClick.AddListener(LoginDevice2);
        }

        protected override void OnDisableScreen()
        {
            _logiDevicenButton.onClick.RemoveListener(LoginDevice);
            _logiDevicen2Button.onClick.RemoveListener(LoginDevice2);
        }

        private async void LoginDevice()
        {
            var authOperation = MirraCloudSDK.Authentication.LoginDeviceAsync(SystemInfo.deviceUniqueIdentifier);

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
        
        private async void LoginDevice2()
        {
            var authOperation = MirraCloudSDK.Authentication.LoginDeviceAsync("2222222222");

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
