using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoginScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _logiDevicenButton;

        protected override void OnEnableScreen()
        {
            _logiDevicenButton.onClick.AddListener(LoginDevice);
        }

        protected override void OnDisableScreen()
        {
            _logiDevicenButton.onClick.RemoveListener(LoginDevice);
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
    }
}
