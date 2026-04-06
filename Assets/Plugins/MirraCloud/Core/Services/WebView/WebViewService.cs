using System;
using Plugins.MirraCloud.Core.General.LifeCycle;
using UnityEngine;

namespace MirraCloud.Core.WebView
{
    public sealed class WebViewService : ICloudSdkService
    {
        private WebViewBridge _bridge;

        public event Action<string> OnMessageReceived;
        public event Action<string> OnError;
        public event Action<string> OnHttpError;
        public event Action<string> OnPageStarted;
        public event Action<string> OnPageLoaded;
        public event Action<string> OnUrlHooked;

        public bool IsReady => _bridge != null && _bridge.IsReady;

        public void CloudSdkInitialize()
        {
            _bridge = WebViewBridge.CreateInstance();
            _bridge.Init();

            _bridge.OnMessageReceived += HandleMessageReceived;
            _bridge.OnError += HandleError;
            _bridge.OnHttpError += HandleHttpError;
            _bridge.OnPageStarted += HandlePageStarted;
            _bridge.OnPageLoaded += HandlePageLoaded;
            _bridge.OnUrlHooked += HandleUrlHooked;
        }

        public void CloudSdkDispose()
        {
            if (_bridge != null)
            {
                _bridge.OnMessageReceived -= HandleMessageReceived;
                _bridge.OnError -= HandleError;
                _bridge.OnHttpError -= HandleHttpError;
                _bridge.OnPageStarted -= HandlePageStarted;
                _bridge.OnPageLoaded -= HandlePageLoaded;
                _bridge.OnUrlHooked -= HandleUrlHooked;

                UnityEngine.Object.Destroy(_bridge.gameObject);
                _bridge = null;
            }
        }

        public void LoadUrl(string url)
        {
            if (!IsReady) return;
            _bridge.Raw.LoadURL(url);
        }

        public void LoadHtml(string html, string baseUrl = null)
        {
            if (!IsReady) return;
            _bridge.Raw.LoadHTML(html, baseUrl);
        }

        public void EvaluateJS(string script)
        {
            if (!IsReady) return;
            _bridge.Raw.EvaluateJS(script);
        }

        public void SetVisibility(bool visible)
        {
            if (!IsReady) return;
            _bridge.Raw.SetVisibility(visible);
        }

        public void SetMargins(int left, int top, int right, int bottom)
        {
            if (!IsReady) return;
            _bridge.Raw.SetMargins(left, top, right, bottom);
        }

        public void GoBack()
        {
            if (!IsReady) return;
            _bridge.Raw.GoBack();
        }

        public void GoForward()
        {
            if (!IsReady) return;
            _bridge.Raw.GoForward();
        }

        public bool CanGoBack()
        {
            if (!IsReady) return false;
            return _bridge.Raw.CanGoBack();
        }

        public bool CanGoForward()
        {
            if (!IsReady) return false;
            return _bridge.Raw.CanGoForward();
        }

        private void HandleMessageReceived(string msg) => OnMessageReceived?.Invoke(msg);
        private void HandleError(string err) => OnError?.Invoke(err);
        private void HandleHttpError(string err) => OnHttpError?.Invoke(err);
        private void HandlePageStarted(string url) => OnPageStarted?.Invoke(url);
        private void HandlePageLoaded(string url) => OnPageLoaded?.Invoke(url);
        private void HandleUrlHooked(string url) => OnUrlHooked?.Invoke(url);
    }
}
