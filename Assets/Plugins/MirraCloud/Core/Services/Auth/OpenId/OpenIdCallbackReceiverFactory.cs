using MirraCloud.Core.WebView;

namespace MirraCloud.Core.Auth.OpenId
{
    internal static class OpenIdCallbackReceiverFactory
    {
        public static bool TryCreate(OpenIdLoginOptions options, WebViewService webView, out IOpenIdCallbackReceiver receiver, out string error)
        {
            receiver = null;
            error = null;

            if (options != null && options.UseInAppWebView)
            {
                if (webView == null || !webView.IsReady)
                {
                    error = "OpenId in-app WebView mode requires an initialized WebViewService.";
                    return false;
                }

                var callbackUrl = string.IsNullOrWhiteSpace(options.WebViewCallbackUrl)
                    ? OpenIdLoginOptions.DefaultWebViewCallbackUrl
                    : options.WebViewCallbackUrl;

                receiver = new WebViewOpenIdReceiver(webView, callbackUrl);
                return true;
            }

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
