using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public struct AuthResponse
    {
        public string token;
        public PlayerAccountInfoResponse playerInfo;

    
    }
}