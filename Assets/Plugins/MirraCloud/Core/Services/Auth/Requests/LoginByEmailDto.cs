using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class LoginByEmailDto
    {
        public string Email;
        public string Password;
        public bool CreateAccount;
    }
}

