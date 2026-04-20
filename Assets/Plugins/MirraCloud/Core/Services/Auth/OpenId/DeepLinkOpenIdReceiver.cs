#if UNITY_ANDROID || UNITY_IOS
using System;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine;

namespace MirraCloud.Core.Auth.OpenId
{
    internal sealed class DeepLinkOpenIdReceiver : IOpenIdCallbackReceiver
    {
        private readonly string _successUrl;
        private readonly AsyncOperation<string> _keyOp = new AsyncOperation<string>();
        private bool _isWaiting;

        public string SuccessUrl => _successUrl;

        public DeepLinkOpenIdReceiver(string successUrl)
        {
            _successUrl = successUrl;
        }

        public bool LaunchAuthUrl(string authUrl)
        {
            if (string.IsNullOrEmpty(authUrl))
            {
                return false;
            }

            Application.OpenURL(authUrl);
            return true;
        }

        public AsyncOperation<string> WaitForKeyAsync()
        {
            if (_isWaiting)
            {
                return _keyOp;
            }

            _isWaiting = true;
            Application.deepLinkActivated += OnDeepLinkActivated;

            if (OpenIdCallbackUrlParser.TryGetOpenIdKey(Application.absoluteURL, out var key))
            {
                Complete(key);
            }

            return _keyOp;
        }

        public void Dispose()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
        }

        private void OnDeepLinkActivated(string url)
        {
            if (OpenIdCallbackUrlParser.TryGetOpenIdKey(url, out var key))
            {
                Complete(key);
            }
        }

        private void Complete(string key)
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
            _keyOp.Complete(key);
        }
    }
}
#endif

