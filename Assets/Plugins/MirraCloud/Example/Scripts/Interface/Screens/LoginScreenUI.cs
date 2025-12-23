using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoginScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _logiDevicenButton;
        [SerializeField] private Button _logiGuestButton;

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
            else if (authOperation.Result.IsSuccess == false)
            {
                UIController.ShowPopup<NetworkErrorPopupUI>();
            }
        }
        
        private async void LoginGuest()
        {
            var authOperation = MirraCloudSDK.Authentication.LoginGuestAsync();

            await authOperation.Task;

            if (MirraCloudSDK.Authentication.IsAuth)
            {
                UIController.ShowScreen<LoadingScreenUI>();
                
                await MirraCloudSDK.RuleConstructor.LoadConfigAsync().Task;
                await MirraCloudSDK.Segments.LoadConfigAsync().Task;
            
            }
            else if (authOperation.Result.IsSuccess == false)
            {
                UIController.ShowPopup<NetworkErrorPopupUI>();
            }
        }
    }
}
