using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class LoginByGoogleDto
    {
        public string UserId;
        public bool CreateAccount;
    }
}

