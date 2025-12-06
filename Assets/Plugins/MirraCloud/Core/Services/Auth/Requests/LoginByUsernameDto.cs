using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class LoginByUsernameDto
    {
        public string Login;
        public string Password;
        public bool CreateAccount;
    }
}

