using MirraCloud.Core;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class WebViewTest : MonoBehaviour
    {
        [Header("URL to load")]
        [SerializeField] private string _url = "https://example.com";
        [SerializeField] private string _anotherUrl = "https://example.com";
        
        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        private void OnEnable()
        {
            _sdk.WebView.OnMessageReceived += HandleMessageReceived;
            _sdk.WebView.OnPageStarted += HandlePageStarted;
            _sdk.WebView.OnPageLoaded += HandlePageLoaded;
            _sdk.WebView.OnError += HandleError;
            _sdk.WebView.OnHttpError += HandleHttpError;
            _sdk.WebView.OnUrlHooked += HandleUrlHooked;
        }

        private void OnDisable()
        {
            _sdk.WebView.OnMessageReceived -= HandleMessageReceived;
            _sdk.WebView.OnPageStarted -= HandlePageStarted;
            _sdk.WebView.OnPageLoaded -= HandlePageLoaded;
            _sdk.WebView.OnError -= HandleError;
            _sdk.WebView.OnHttpError -= HandleHttpError;
            _sdk.WebView.OnUrlHooked -= HandleUrlHooked;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) LoadUrl();
            if (Input.GetKeyDown(KeyCode.Alpha9)) LoadAnotherUrl();
            if (Input.GetKeyDown(KeyCode.Alpha2)) Show();
            if (Input.GetKeyDown(KeyCode.Alpha3)) Hide();
            if (Input.GetKeyDown(KeyCode.Alpha4)) GoBack();
            if (Input.GetKeyDown(KeyCode.Alpha5)) GoForward();
            if (Input.GetKeyDown(KeyCode.Alpha6)) RunJS();
        }

        private void LoadUrl()
        {
            Debug.Log($"[WebViewTest] Loading URL: {_url}");
            _sdk.WebView.SetMargins(50, 100, 50, 100);
            _sdk.WebView.SetVisibility(true);
            _sdk.WebView.LoadUrl(_url);
        }
        
        private void LoadAnotherUrl()
        {
            Debug.Log($"[WebViewTest] Loading URL: {_anotherUrl}");
            _sdk.WebView.SetMargins(50, 100, 50, 100);
            _sdk.WebView.SetVisibility(true);
            _sdk.WebView.LoadUrl(_anotherUrl);
        }

        private void Show()
        {
            Debug.Log("[WebViewTest] Show");
            _sdk.WebView.SetVisibility(true);
        }

        private void Hide()
        {
            Debug.Log("[WebViewTest] Hide");
            _sdk.WebView.SetVisibility(false);
        }

        private void GoBack()
        {
            if (_sdk.WebView.CanGoBack())
            {
                Debug.Log("[WebViewTest] GoBack");
                _sdk.WebView.GoBack();
            }
            else
            {
                Debug.LogWarning("[WebViewTest] Cannot go back.");
            }
        }

        private void GoForward()
        {
            if (_sdk.WebView.CanGoForward())
            {
                Debug.Log("[WebViewTest] GoForward");
                _sdk.WebView.GoForward();
            }
            else
            {
                Debug.LogWarning("[WebViewTest] Cannot go forward.");
            }
        }

        private void RunJS()
        {
            Debug.Log("[WebViewTest] Evaluating JS: document.title");
            _sdk.WebView.EvaluateJS("Unity.call(document.title)");
        }

        private void HandleMessageReceived(string msg) => Debug.Log($"[WebViewTest] JS message: {msg}");
        private void HandlePageStarted(string url) => Debug.Log($"[WebViewTest] Started: {url}");
        private void HandlePageLoaded(string url) => Debug.Log($"[WebViewTest] Loaded: {url}");
        private void HandleError(string err) => Debug.LogError($"[WebViewTest] Error: {err}");
        private void HandleHttpError(string err) => Debug.LogError($"[WebViewTest] HTTP Error: {err}");
        private void HandleUrlHooked(string url) => Debug.Log($"[WebViewTest] Hooked: {url}");
    }
}
