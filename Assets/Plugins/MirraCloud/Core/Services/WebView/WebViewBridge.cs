using System;
using UnityEngine;

namespace MirraCloud.Core.WebView
{
    internal sealed class WebViewBridge : MonoBehaviour
    {
        private WebViewObject _webViewObject;

        public event Action<string> OnMessageReceived;
        public event Action<string> OnError;
        public event Action<string> OnHttpError;
        public event Action<string> OnPageStarted;
        public event Action<string> OnPageLoaded;
        public event Action<string> OnUrlHooked;

        public bool IsReady { get; private set; }

        public static WebViewBridge CreateInstance()
        {
            var go = new GameObject("MirraCloudSDK WebView");
            DontDestroyOnLoad(go);
            return go.AddComponent<WebViewBridge>();
        }

        public void Init()
        {
#if UNITY_WEBGL && UNITY_EDITOR
            Debug.LogWarning("[MirraCloud] WebView is not supported in Editor with WebGL target. Switch to Standalone for testing.");
            return;
#else
            _webViewObject = gameObject.AddComponent<WebViewObject>();
            _webViewObject.Init(
                cb: msg => OnMessageReceived?.Invoke(msg),
                err: err => OnError?.Invoke(err),
                httpErr: err => OnHttpError?.Invoke(err),
                started: url => OnPageStarted?.Invoke(url),
                ld: url => OnPageLoaded?.Invoke(url),
                hooked: url => OnUrlHooked?.Invoke(url),
                enableWKWebView: true
            );
            _webViewObject.SetVisibility(false);
            IsReady = true;
#endif
        }

        public WebViewObject Raw => _webViewObject;

        private void OnDestroy()
        {
            if (_webViewObject != null)
                Destroy(_webViewObject);
        }
    }
}
