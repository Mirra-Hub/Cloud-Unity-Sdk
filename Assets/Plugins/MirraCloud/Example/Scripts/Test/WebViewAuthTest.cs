using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.Auth;
using MirraCloud.Core.Auth.OpenId;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class WebViewAuthTest : MonoBehaviour
    {
        public enum AuthMethod
        {
            // LoginOpenIdAsync — full OAuth/OIDC flow against a custom OpenID provider
            // configured in the project admin. Identified by numeric providerId.
            OpenId = 0,
            // LoginPlatformAsync — login through a backend Platform entity (Google Play,
            // Apple Game Center, Sign In With Apple, Google Sign-In, VK Games, Yandex Games,
            // Yandex ID). Identified by the Platform's ObjectId. Requires marketplace-specific
            // fields (externalUserId / authCode / platformToken / extra) to be filled.
            Platform = 1,
        }

        public enum OpenIdTransport
        {
            // Let OpenIdCallbackReceiverFactory pick the receiver based on compile-time
            // defines: loopback on Editor/Standalone, deep link on Android/iOS.
            Auto = 0,
            // In-app WebView. Editor/Standalone/Android/iOS, no deep-link setup required.
            InAppWebView = 1,
            // System browser + local HTTP loopback listener. Editor/Standalone only.
            DesktopLoopback = 2,
            // System browser + custom-scheme deep link callback. Android/iOS only.
            MobileDeepLink = 3,
        }

        [Serializable]
        private struct ExtraEntry
        {
            public string Key;
            public string Value;
        }

        [Header("Auth method")]
        [Tooltip("Which SDK call to make on F1.")]
        [SerializeField] private AuthMethod _method = AuthMethod.OpenId;

        [Header("Common")]
        [SerializeField] private bool _createAccount = true;
        [Tooltip("Nickname used by the server only when CreateAccount=true and the account does not yet exist.")]
        [SerializeField] private string _nickname;

        [Header("OpenID provider (method = OpenId)")]
        [Tooltip("Numeric id of the OpenId provider registered for this project via PlayerAccounts admin API.")]
        [SerializeField] private int _providerId;
        [Tooltip("Transport used to display the OAuth page and intercept the callback.")]
        [SerializeField] private OpenIdTransport _transport = OpenIdTransport.InAppWebView;
        [SerializeField] private string _webViewCallbackUrl = OpenIdLoginOptions.DefaultWebViewCallbackUrl;
        [Tooltip("Margins (left, top, right, bottom) applied to the in-app WebView when it is shown for the OpenID auth page.")]
        [SerializeField] private Vector4 _webViewMargins = new Vector4(50, 100, 50, 100);
        [Tooltip("Local TCP port for the loopback HTTP listener. 0 = auto-pick a free port.")]
        [SerializeField] private int _loopbackPort;
        [Tooltip("Custom-scheme URL the system browser must redirect to (e.g. myapp://mirra-openid).")]
        [SerializeField] private string _mobileDeepLinkUrl;

        [Header("Platform provider (method = Platform)")]
        [Tooltip("ObjectId of the backend Platform entity (admin → Cloud → Platforms). Marketplace type drives which of the fields below are required.")]
        [SerializeField] private string _platformId;
        [Tooltip("Stable user id from the marketplace (e.g. Google Player ID, Apple teamPlayerID, VK user_id). Empty for marketplaces that derive it from a token (GoogleSignIn, SignInWithApple, Yandex ID).")]
        [SerializeField] private string _externalUserId;
        [Tooltip("OAuth code (Yandex ID OAuth from passport.yandex.ru, or Google Play server auth code).")]
        [SerializeField] private string _authCode;
        [Tooltip("Platform-issued signed token: Google ID token (GoogleSignIn), Apple identity JWT (SignInWithApple), Yandex Games signed token, etc.")]
        [SerializeField] private string _platformToken;
        [Tooltip("Free-form key/value pairs (Apple Game Center publicKeyUrl/signature/salt/timestamp, VK Games launch params, etc.).")]
        [SerializeField] private List<ExtraEntry> _extra = new List<ExtraEntry>();

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
            if (Input.GetKeyDown(KeyCode.F1)) Login();
            if (Input.GetKeyDown(KeyCode.F2)) CheckAuthState();
            if (Input.GetKeyDown(KeyCode.F3)) Logout();
            if (Input.GetKeyDown(KeyCode.F4)) TestWebViewReuse();
        }

        private void Login()
        {
            switch (_method)
            {
                case AuthMethod.OpenId:
                    LoginOpenId();
                    break;
                case AuthMethod.Platform:
                    LoginPlatform();
                    break;
            }
        }

        private void LoginOpenId()
        {
            Debug.Log($"[WebViewAuthTest] LoginOpenId | providerId={_providerId} transport={_transport}");

            var options = BuildOpenIdOptions();

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
            op.UseCompleted(completed => LogResult("LoginOpenId", completed.Result));
        }

        private void LoginPlatform()
        {
            Debug.Log($"[WebViewAuthTest] LoginPlatform | platformId={_platformId} externalUserId={_externalUserId}");

            if (string.IsNullOrWhiteSpace(_platformId))
            {
                Debug.LogError("[WebViewAuthTest] PlatformId is empty — fill it in the inspector.");
                return;
            }

            var op = _sdk.Authentication.LoginPlatformAsync(
                platformId: _platformId,
                externalUserId: _externalUserId,
                authCode: NullIfEmpty(_authCode),
                platformToken: NullIfEmpty(_platformToken),
                extra: BuildExtra(),
                createAccount: _createAccount,
                nickname: NullIfEmpty(_nickname));

            op.UseCompleted(completed => LogResult("LoginPlatform", completed.Result));
        }

        private OpenIdLoginOptions BuildOpenIdOptions()
        {
            switch (_transport)
            {
                case OpenIdTransport.InAppWebView:
                    return new OpenIdLoginOptions
                    {
                        UseInAppWebView = true,
                        WebViewCallbackUrl = string.IsNullOrWhiteSpace(_webViewCallbackUrl)
                            ? OpenIdLoginOptions.DefaultWebViewCallbackUrl
                            : _webViewCallbackUrl,
                    };

                case OpenIdTransport.DesktopLoopback:
                    return new OpenIdLoginOptions
                    {
                        UseInAppWebView = false,
                        LoopbackPort = _loopbackPort,
                    };

                case OpenIdTransport.MobileDeepLink:
                    return new OpenIdLoginOptions
                    {
                        UseInAppWebView = false,
                        MobileDeepLinkUrl = _mobileDeepLinkUrl,
                    };

                case OpenIdTransport.Auto:
                default:
                    return new OpenIdLoginOptions
                    {
                        UseInAppWebView = false,
                        LoopbackPort = _loopbackPort,
                        MobileDeepLinkUrl = _mobileDeepLinkUrl,
                    };
            }
        }

        private Dictionary<string, string> BuildExtra()
        {
            if (_extra == null || _extra.Count == 0) return null;

            var dict = new Dictionary<string, string>(_extra.Count);
            foreach (var entry in _extra)
            {
                if (string.IsNullOrEmpty(entry.Key)) continue;
                dict[entry.Key] = entry.Value;
            }
            return dict.Count > 0 ? dict : null;
        }

        private void LogResult(string label, RestApiResult<GetAuthDataDto> result)
        {
            if (result.IsSuccess)
            {
                var data = result.Data;
                Debug.Log($"[WebViewAuthTest] {label} SUCCESS | status={data.Status} token={Truncate(data.Token)} sessionId={data.Session?.SessionId}");
            }
            else
            {
                Debug.LogError($"[WebViewAuthTest] {label} FAILED | {result.Error?.Message}");
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

        private static string NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

        private static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value)) return "(null)";
            return value.Length > 16 ? value.Substring(0, 16) + "..." : value;
        }
    }
}
