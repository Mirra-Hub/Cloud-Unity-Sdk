using System;

namespace MirraCloud
{
    [Serializable]
    public struct LoginWithWebIDRequest
    {
        public string Id;
        public bool CreateAccount;
    }
}