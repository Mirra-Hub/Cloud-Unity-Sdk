using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class LoginByDeviceIdDto
    {
        public string DeviceId;
        public bool CreateAccount;
    }
}

