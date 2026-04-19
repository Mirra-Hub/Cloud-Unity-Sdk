using System;
using System.Collections.Generic;

namespace MirraCloud.Core.Auth
{
    /// <summary>
    /// DTO for /login/sign-in. Identifies the player by (SignInProviderId, ExternalUserId)
    /// and optionally carries provider-specific verification data:
    /// - AuthCode for OAuth code-flow providers (Yandex ID),
    /// - IdToken for ID-token providers (Google Sign-In, Sign In With Apple),
    /// - Extra for additional native data (Apple Game Center signature payload).
    /// </summary>
    [Serializable]
    public class LoginBySignInProviderDto
    {
        public string SignInProviderId;
        public string ExternalUserId;
        public string AuthCode;
        public string IdToken;
        public Dictionary<string, string> Extra;
        public bool CreateAccount;
    }
}
