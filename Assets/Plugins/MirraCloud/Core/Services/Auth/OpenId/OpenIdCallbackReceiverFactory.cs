namespace MirraCloud.Core.Auth.OpenId
{
    internal static class OpenIdCallbackReceiverFactory
    {
        public static bool TryCreate(OpenIdLoginOptions options, out IOpenIdCallbackReceiver receiver, out string error)
        {
            receiver = null;
            error = null;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (LoopbackOpenIdReceiver.TryCreate(options?.LoopbackPort ?? 0, out receiver, out error))
            {
                return true;
            }

            return false;
#elif UNITY_ANDROID || UNITY_IOS
            var deepLinkUrl = options?.MobileDeepLinkUrl;
            if (string.IsNullOrWhiteSpace(deepLinkUrl))
            {
                error = "OpenId login on mobile requires OpenIdLoginOptions.MobileDeepLinkUrl (e.g. myapp://mirra-openid).";
                return false;
            }

            receiver = new DeepLinkOpenIdReceiver(deepLinkUrl);
            return true;
#else
            error = "OpenId login receiver is not supported on this platform yet.";
            return false;
#endif
        }
    }
}

