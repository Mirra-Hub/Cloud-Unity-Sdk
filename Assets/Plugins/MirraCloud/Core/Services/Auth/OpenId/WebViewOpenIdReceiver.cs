using System;
using MirraCloud.Core.WebView;
using MirraCloud.Core.WebView.Dispatching;
using MirraCloud.Core.WebView.Protocol;
using Plugins.MirraCloud.Core.General.AsyncOperations;

namespace MirraCloud.Core.Auth.OpenId
{
    internal sealed class WebViewOpenIdReceiver : IOpenIdCallbackReceiver
    {
        private const string KeyParamName = "mirra_openid_key";

        private readonly WebViewService _webView;
        private readonly string _callbackUrl;
        private readonly AsyncOperation<string> _keyOp = new AsyncOperation<string>();

        private bool _disposed;
        private bool _completed;

        public string SuccessUrl => _callbackUrl;

        public WebViewOpenIdReceiver(WebViewService webView, string callbackUrl)
        {
            _webView = webView;
            _callbackUrl = callbackUrl;

            _webView.RegisterCallbackHandler(_callbackUrl, new KeyHandler(KeyParamName, CompleteWith));
        }

        public bool LaunchAuthUrl(string authUrl)
        {
            if (_disposed || _completed)
            {
                return false;
            }

            if (string.IsNullOrEmpty(authUrl))
            {
                return false;
            }

            if (!_webView.IsReady)
            {
                return false;
            }

            _webView.ActivateHookPattern();
            _webView.SetVisibility(true);
            _webView.LoadUrl(authUrl);
            return true;
        }

        public AsyncOperation<string> WaitForKeyAsync()
        {
            return _keyOp;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try { _webView.ClearCallbackHandlers(); } catch { /* ignored */ }
            try { _webView.SetVisibility(false); } catch { /* ignored */ }

            if (!_completed)
            {
                _completed = true;
                _keyOp.Complete(null);
            }
        }

        private void CompleteWith(string key)
        {
            if (_completed || _disposed)
            {
                return;
            }

            try { _webView.LoadUrl("about:blank"); } catch { /* ignored */ }

            _completed = true;
            _keyOp.Complete(key);
        }

        private sealed class KeyHandler : IWebViewCallbackHandler
        {
            private readonly string _paramName;
            private readonly Action<string> _onMatch;

            public KeyHandler(string paramName, Action<string> onMatch)
            {
                _paramName = paramName;
                _onMatch = onMatch;
            }

            public void Handle(WebViewCallbackEnvelope envelope)
            {
                envelope.QueryParams.TryGetValue(_paramName, out var key);
                _onMatch(string.IsNullOrWhiteSpace(key) ? null : key);
            }
        }
    }
}
