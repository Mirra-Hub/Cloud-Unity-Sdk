using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class LoginByOpenIdDto
    {
        public string UserId;
        public bool CreateAccount;
    }
}

