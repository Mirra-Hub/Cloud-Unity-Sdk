using MirraCloud.Core;
using MirraCloud.Core.Auth;
using MirraCloud.Core.Auth.OpenId;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class WebViewAuthTest : MonoBehaviour
    {
        [Header("OpenID Provider")]
        [Tooltip("Numeric id of the OpenId provider registered for this project via PlayerAccounts admin API.")]
        [SerializeField] private int _providerId;

        [Header("WebView")]
        [SerializeField] private bool _useWebView = true;
        [SerializeField] private string _callbackUrl = OpenIdLoginOptions.DefaultWebViewCallbackUrl;

        [Header("Fallback (system browser)")]
        [SerializeField] private string _mobileDeepLinkUrl;
        [SerializeField] private int _loopbackPort;

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
            if (Input.GetKeyDown(KeyCode.F2)) LoginOpenIdSystemBrowser();
            if (Input.GetKeyDown(KeyCode.F3)) CheckAuthState();
            if (Input.GetKeyDown(KeyCode.F4)) Logout();
            if (Input.GetKeyDown(KeyCode.F5)) TestWebViewReuse();
        }

        private void LoginOpenId()
        {
            Debug.Log($"[WebViewAuthTest] LoginOpenId via WebView | providerId={_providerId}");

            var options = new OpenIdLoginOptions
            {
                UseInAppWebView = _useWebView,
                WebViewCallbackUrl = _callbackUrl,
                MobileDeepLinkUrl = _mobileDeepLinkUrl,
                LoopbackPort = _loopbackPort
            };

            var op = GetLoginOperation(options);
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

        private void LoginOpenIdSystemBrowser()
        {
            Debug.Log($"[WebViewAuthTest] LoginOpenId via system browser | providerId={_providerId}");

            var options = new OpenIdLoginOptions
            {
                UseInAppWebView = false,
                MobileDeepLinkUrl = _mobileDeepLinkUrl,
                LoopbackPort = _loopbackPort
            };

            var op = GetLoginOperation(options);
            op.UseCompleted(completed =>
            {
                var result = completed.Result;
                if (result.IsSuccess)
                {
                    Debug.Log($"[WebViewAuthTest] Login SUCCESS (browser) | status={result.Data.Status}");
                }
                else
                {
                    Debug.LogError($"[WebViewAuthTest] Login FAILED (browser) | {result.Error?.Message}");
                }
            });
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

        private Plugins.MirraCloud.Core.General.AsyncOperations.AsyncOperation<RestApiResult<GetAuthDataDto>> GetLoginOperation(OpenIdLoginOptions options)
        {
            return _sdk.Authentication.LoginOpenIdAsync(_providerId, options);
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
