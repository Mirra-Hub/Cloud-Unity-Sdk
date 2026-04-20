using System;
using System.Text.RegularExpressions;
using MirraCloud.Core.WebView;
using Plugins.MirraCloud.Core.General.AsyncOperations;

namespace MirraCloud.Core.Auth.OpenId
{
    internal sealed class WebViewOpenIdReceiver : IOpenIdCallbackReceiver
    {
        private readonly WebViewService _webView;
        private readonly string _callbackUrl;
        private readonly string _callbackUrlNormalized;
        private readonly AsyncOperation<string> _keyOp = new AsyncOperation<string>();

        private bool _disposed;
        private bool _completed;

        public string SuccessUrl => _callbackUrl;

        public WebViewOpenIdReceiver(WebViewService webView, string callbackUrl)
        {
            _webView = webView;
            _callbackUrl = callbackUrl;
            _callbackUrlNormalized = NormalizeForPrefixCompare(callbackUrl);

            _webView.OnUrlHooked += HandleCallbackUrl;
            _webView.OnPageStarted += HandleCallbackUrl;
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

            var hookRegex = "^" + Regex.Escape(_callbackUrl);
            _webView.SetUrlPattern(null, null, hookRegex);

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

            try { _webView.OnUrlHooked -= HandleCallbackUrl; } catch { /* ignored */ }
            try { _webView.OnPageStarted -= HandleCallbackUrl; } catch { /* ignored */ }
            try { _webView.SetUrlPattern(null, null, null); } catch { /* ignored */ }
            try { _webView.SetVisibility(false); } catch { /* ignored */ }

            if (!_completed)
            {
                _completed = true;
                _keyOp.Complete(null);
            }
        }

        private void HandleCallbackUrl(string url)
        {
            if (_completed || _disposed)
            {
                return;
            }

            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var normalized = NormalizeForPrefixCompare(url);
            if (!normalized.StartsWith(_callbackUrlNormalized, StringComparison.Ordinal))
            {
                return;
            }

            try { _webView.LoadUrl("about:blank"); } catch { /* ignored */ }

            _completed = true;

            if (OpenIdCallbackUrlParser.TryGetOpenIdKey(url, out var key))
            {
                _keyOp.Complete(key);
            }
            else
            {
                _keyOp.Complete(null);
            }
        }

        private static string NormalizeForPrefixCompare(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            var questionIndex = url.IndexOf('?');
            var basePart = questionIndex >= 0 ? url.Substring(0, questionIndex) : url;
            return basePart.TrimEnd('/').ToLowerInvariant();
        }
    }
}
