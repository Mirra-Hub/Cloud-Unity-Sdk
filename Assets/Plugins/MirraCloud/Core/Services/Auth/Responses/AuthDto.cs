using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public struct AuthDto
    {
        public string token;
        public PlayerAccountInfoDto playerInfo;
    }
}