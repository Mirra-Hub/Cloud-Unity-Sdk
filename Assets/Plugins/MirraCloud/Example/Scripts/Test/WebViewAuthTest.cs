using MirraCloud.Core;
using MirraCloud.Core.Auth;
using MirraCloud.Core.Auth.OpenId;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class WebViewAuthTest : MonoBehaviour
    {
        public enum OpenIdAuthPlatform
        {
            // Let OpenIdCallbackReceiverFactory pick the receiver based on compile-time defines:
            // loopback on Editor/Standalone, deep link on Android/iOS.
            Auto = 0,
            // In-app WebView. Works on Editor/Standalone/Android/iOS, no deep-link setup required.
            InAppWebView = 1,
            // System browser + local HTTP loopback listener. Editor/Standalone only.
            DesktopLoopback = 2,
            // System browser + custom-scheme deep link callback. Android/iOS only.
            MobileDeepLink = 3,
        }

        [Header("OpenID Provider")]
        [Tooltip("Numeric id of the OpenId provider registered for this project via PlayerAccounts admin API.")]
        [SerializeField] private int _providerId;

        [Header("Auth Platform")]
        [Tooltip("Transport used to display the OAuth page and intercept the callback.")]
        [SerializeField] private OpenIdAuthPlatform _platform = OpenIdAuthPlatform.InAppWebView;

        [Header("InAppWebView settings")]
        [SerializeField] private string _webViewCallbackUrl = OpenIdLoginOptions.DefaultWebViewCallbackUrl;
        [Tooltip("Margins (left, top, right, bottom) applied to the in-app WebView when it is shown for the OpenID auth page.")]
        [SerializeField] private Vector4 _webViewMargins = new Vector4(50, 100, 50, 100);

        [Header("DesktopLoopback settings")]
        [Tooltip("Local TCP port for the loopback HTTP listener. 0 = auto-pick a free port.")]
        [SerializeField] private int _loopbackPort;

        [Header("MobileDeepLink settings")]
        [Tooltip("Custom-scheme URL the system browser must redirect to (e.g. myapp://mirra-openid).")]
        [SerializeField] private string _mobileDeepLinkUrl;

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        private void OnEnable()
        {
            _sdk.Authentication.OnLogin += HandleLogin;
            _sdk.Authentication.OnAuthConflict += HandleConflict;
            _sdk.Authentication.OnSessionExpired += HandleSessionExpired;
        }

        private void OnDisable()
        {
            _sdk.Authentication.OnLogin -= HandleLogin;
            _sdk.Authentication.OnAuthConflict -= HandleConflict;
            _sdk.Authentication.OnSessionExpired -= HandleSessionExpired;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) LoginOpenId();
            if (Input.GetKeyDown(KeyCode.F2)) CheckAuthState();
            if (Input.GetKeyDown(KeyCode.F3)) Logout();
            if (Input.GetKeyDown(KeyCode.F4)) TestWebViewReuse();
        }

        private void LoginOpenId()
        {
            Debug.Log($"[WebViewAuthTest] LoginOpenId | providerId={_providerId} platform={_platform}");

            var options = BuildOptions();

            // The SDK only flips the WebView visibility on; the host app is responsible for sizing it.
            // Without explicit margins the native WebView may render at zero size, making the auth page
            // invisible — that is exactly why login through providers used to silently fail here.
            if (options.UseInAppWebView && _sdk.WebView != null && _sdk.WebView.IsReady)
            {
                _sdk.WebView.SetMargins(
                    (int)_webViewMargins.x,
                    (int)_webViewMargins.y,
                    (int)_webViewMargins.z,
                    (int)_webViewMargins.w);
            }

            var op = _sdk.Authentication.LoginOpenIdAsync(_providerId, options);
            op.UseCompleted(completed =>
            {
                var result = completed.Result;
                if (result.IsSuccess)
                {
                    var data = result.Data;
                    Debug.Log($"[WebViewAuthTest] Login SUCCESS | status={data.Status} token={Truncate(data.Token)} sessionId={data.Session?.SessionId}");
                }
                else
                {
                    Debug.LogError($"[WebViewAuthTest] Login FAILED | {result.Error?.Message}");
                }
            });
        }

        private OpenIdLoginOptions BuildOptions()
        {
            switch (_platform)
            {
                case OpenIdAuthPlatform.InAppWebView:
                    return new OpenIdLoginOptions
                    {
                        UseInAppWebView = true,
                        WebViewCallbackUrl = string.IsNullOrWhiteSpace(_webViewCallbackUrl)
                            ? OpenIdLoginOptions.DefaultWebViewCallbackUrl
                            : _webViewCallbackUrl,
                    };

                case OpenIdAuthPlatform.DesktopLoopback:
                    return new OpenIdLoginOptions
                    {
                        UseInAppWebView = false,
                        LoopbackPort = _loopbackPort,
                    };

                case OpenIdAuthPlatform.MobileDeepLink:
                    return new OpenIdLoginOptions
                    {
                        UseInAppWebView = false,
                        MobileDeepLinkUrl = _mobileDeepLinkUrl,
                    };

                case OpenIdAuthPlatform.Auto:
                default:
                    // Hand both desktop and mobile fields to the factory; it picks one by compile-time defines.
                    return new OpenIdLoginOptions
                    {
                        UseInAppWebView = false,
                        LoopbackPort = _loopbackPort,
                        MobileDeepLinkUrl = _mobileDeepLinkUrl,
                    };
            }
        }

        private void CheckAuthState()
        {
            Debug.Log($"[WebViewAuthTest] IsAuth={_sdk.Authentication.IsAuth} SessionId={_sdk.Authentication.SessionId} Token={Truncate(_sdk.Authentication.AuthToken)}");
        }

        private void Logout()
        {
            Debug.Log("[WebViewAuthTest] Logging out...");
            var op = _sdk.Authentication.LogoutAsync();
            op.UseCompleted(completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log("[WebViewAuthTest] Logout SUCCESS");
                }
                else
                {
                    Debug.LogError($"[WebViewAuthTest] Logout FAILED | {completed.Result.Error?.Message}");
                }
            });
        }

        private void TestWebViewReuse()
        {
            Debug.Log("[WebViewAuthTest] Testing WebView reuse after auth (loading example.com)...");

            if (!_sdk.WebView.IsReady)
            {
                Debug.LogError("[WebViewAuthTest] WebView is not ready");
                return;
            }

            _sdk.WebView.SetMargins(50, 100, 50, 100);
            _sdk.WebView.SetVisibility(true);
            _sdk.WebView.LoadUrl("https://example.com");
            Debug.Log("[WebViewAuthTest] WebView should show example.com without URL interception");
        }

        private void HandleLogin(GetAuthDataDto data)
        {
            Debug.Log($"[WebViewAuthTest] EVENT OnLogin | status={data.Status} playerId={data.PlayerInfo?.Id}");
        }

        private void HandleConflict(GetAuthDataDto data)
        {
            Debug.LogWarning($"[WebViewAuthTest] EVENT OnAuthConflict | current={data.CurrentAccount?.Id} existing={data.ExistingAccount?.Id}");
        }

        private void HandleSessionExpired()
        {
            Debug.LogWarning("[WebViewAuthTest] EVENT OnSessionExpired");
        }

        private static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value)) return "(null)";
            return value.Length > 16 ? value.Substring(0, 16) + "..." : value;
        }
    }
}
