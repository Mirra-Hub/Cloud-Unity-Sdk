using System;

namespace MirraCloud.Core.Auth.OpenId
{
    [Serializable]
    public sealed class OpenIdLoginOptions
    {
        public int LoopbackPort;
        public string MobileDeepLinkUrl;
    }
}

