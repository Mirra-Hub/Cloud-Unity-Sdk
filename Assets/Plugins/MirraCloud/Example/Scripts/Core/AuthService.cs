using System.Threading.Tasks;
using MirraCloud.Core;
using UnityEngine.Device;

namespace Plugins.MirraCloud.Example.Scripts.Core
{
    public class AuthService
    {
        public async Task<bool> LoginDevice()
        {
            var authOperation = MirraCloudSDK.Authentication.LoginDeviceAsync(SystemInfo.deviceUniqueIdentifier);

            await authOperation.Task;
            
            return authOperation.Result.IsSuccess;
        }

        public async Task<bool> LoginGuest()
        {
            var authOperation = MirraCloudSDK.Authentication.LoginGuestAsync();

            await authOperation.Task;
            
           return authOperation.Result.IsSuccess;
        }
    }
}