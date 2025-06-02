using System;

namespace MirraCloud
{
    [Serializable]
    public struct LoginWithDeviceIDRequest
    {
        public string DeviceId;
        public bool CreateAccount;
    }
}