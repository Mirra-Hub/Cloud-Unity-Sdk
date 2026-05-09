using System.Threading.Tasks;
using MirraCloud.Core;
using UnityEngine.Device;

namespace Plugins.MirraCloud.Example.Scripts.Core
{
    public class AuthService
    {
        private readonly IMirraCloudSdk _sdk;

        public AuthService(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        public async Task<bool> LoginDevice()
        {
            var authOperation = _sdk.Authentication.LoginDeviceAsync(SystemInfo.deviceUniqueIdentifier);

            await authOperation.Task();

            return _sdk.Authentication.IsAuth;
        }

        public async Task<bool> LoginGuest()
        {
            // Guest login is anonymous — the server generates a nickname based
            // on the project's NicknameSettings, so nothing needs to be passed
            // from the client.
            var authOperation = _sdk.Authentication.LoginGuestAsync();

            await authOperation.Task();

            return _sdk.Authentication.IsAuth;
        }
    }
}
