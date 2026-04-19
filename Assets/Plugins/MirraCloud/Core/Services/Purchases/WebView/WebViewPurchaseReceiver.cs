using System;
using System.Text.RegularExpressions;
using MirraCloud.Core.WebView;
using Plugins.MirraCloud.Core.General.AsyncOperations;

namespace MirraCloud.Core.Purchases.WebView
{
    internal sealed class WebViewPurchaseReceiver : IPurchaseCallbackReceiver
    {
        private readonly WebViewService _webView;
        private readonly string _successUrl;
        private readonly string _cancelUrl;
        private readonly string _successUrlNormalized;
        private readonly string _cancelUrlNormalized;
        private readonly AsyncOperation<PaymentFlowOutcome> _outcomeOp = new AsyncOperation<PaymentFlowOutcome>();

        private bool _disposed;
        private bool _completed;

        public WebViewPurchaseReceiver(WebViewService webView, string successUrl, string cancelUrl)
        {
            _webView = webView;
            _successUrl = successUrl;
            _cancelUrl = cancelUrl;
            _successUrlNormalized = NormalizeForPrefixCompare(successUrl);
            _cancelUrlNormalized = NormalizeForPrefixCompare(cancelUrl);

            _webView.OnUrlHooked += HandleCallbackUrl;
            _webView.OnPageStarted += HandleCallbackUrl;
        }

        public bool LaunchPaymentUrl(string paymentUrl)
        {
            if (_disposed || _completed)
            {
                return false;
            }

            if (string.IsNullOrEmpty(paymentUrl))
            {
                return false;
            }

            if (!_webView.IsReady)
            {
                return false;
            }

            var hookRegex = "^(" + Regex.Escape(_successUrl) + "|" + Regex.Escape(_cancelUrl) + ")";
            _webView.SetUrlPattern(null, null, hookRegex);

            _webView.SetVisibility(true);
            _webView.LoadUrl(paymentUrl);
            return true;
        }

        public AsyncOperation<PaymentFlowOutcome> WaitForOutcomeAsync()
        {
            return _outcomeOp;
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
                _outcomeOp.Complete(PaymentFlowOutcome.Dismissed);
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

            PaymentFlowOutcome outcome;
            if (normalized.StartsWith(_successUrlNormalized, StringComparison.Ordinal))
            {
                outcome = PaymentFlowOutcome.Success;
            }
            else if (normalized.StartsWith(_cancelUrlNormalized, StringComparison.Ordinal))
            {
                outcome = PaymentFlowOutcome.Cancelled;
            }
            else
            {
                return;
            }

            try { _webView.LoadUrl("about:blank"); } catch { /* ignored */ }

            _completed = true;
            _outcomeOp.Complete(outcome);
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
