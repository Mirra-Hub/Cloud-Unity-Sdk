using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public struct LoginWithGuestRequest
    {
        public string GuestId;
        public bool CreateAccount;
    }
}