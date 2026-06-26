using System;
using MirraCloud.Core;
using MirraCloud.Core.Auth.OpenId;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// VContainer entry point for the official example. Builds the screen/overlay/toast hosts,
    /// gates on auth (AuthScreen ↔ ServicesScreen), and routes provider buttons to the SDK.
    /// Login success navigates via Authentication.OnLogin; OnSessionExpired returns to auth.
    /// </summary>
    public sealed class ShowcaseApp : IStartable
    {
        private readonly IMirraCloudSdk _sdk;
        private readonly UIDocument _document;
        private readonly RemoteImageLoader _images;
        private readonly bool _devForceServices;

        private Nav _nav;
        private Popup _popup;
        private Toasts _toasts;

        public ShowcaseApp(IMirraCloudSdk sdk, UIDocument document, RemoteImageLoader images, ShowcaseOptions options)
        {
            _sdk = sdk;
            _document = document;
            _images = images;
            _devForceServices = options != null && options.DevForceServices;
        }

        public void Start()
        {
            if (!_sdk.IsInitialized)
            {
                _sdk.Initialize();
            }

            var root = _document.rootVisualElement;
            var screen = root.Q<VisualElement>("sc-screen") ?? root;
            screen.Clear();
            screen.AddToClassList("sc-root");

            var navHost = New("sc-nav-host");
            screen.Add(navHost);

            var overlay = New("sc-overlay");
            overlay.pickingMode = PickingMode.Ignore; // children (scrim) still receive clicks
            screen.Add(overlay);

            var toastHost = New("sc-toast-host2");
            toastHost.pickingMode = PickingMode.Ignore;
            screen.Add(toastHost);

            _nav = new Nav(navHost);
            _popup = new Popup(overlay);
            _toasts = new Toasts(toastHost);

            _sdk.Authentication.OnLogin += _ => OnLoggedIn();
            _sdk.Authentication.OnSessionExpired += ShowAuth;

            if (_devForceServices || _sdk.Authentication.IsAuth)
            {
                ShowServices();
            }
            else
            {
                ShowAuth();
            }
        }

        private void OnLoggedIn()
        {
            _popup?.Close();
            ShowServices();
        }

        private void ShowAuth()
        {
            string project = "", branch = "", platform = "";
            try
            {
                var cfg = MirraCloud.Configuration.Load();
                if (cfg != null)
                {
                    project = cfg.ProjectId;
                    branch = cfg.BranchId;
                    platform = cfg.AnalyticsPlatformId;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Showcase] config load failed: " + e.Message);
            }

            var auth = new AuthView(_popup, project, branch, platform);
            auth.GuestRequested += () => RunAuth("Guest", _sdk.Authentication.LoginGuestAsync());
            auth.DeviceRequested += () => RunAuth("Device", _sdk.Authentication.LoginDeviceAsync(SystemInfo.deviceUniqueIdentifier));
            auth.EmailLoginRequested += (email, pass) => RunAuth("Email", _sdk.Authentication.LoginEmailAsync(email, pass));
            auth.OpenIdRequested += id =>
            {
                _toasts.Info("Opening provider…");
                RunAuth("Provider", _sdk.Authentication.LoginOpenIdAsync(id, new OpenIdLoginOptions { UseInAppWebView = true }));
            };
            _nav.SetRoot(auth);
        }

        private void ShowServices()
        {
            var services = new ServicesView(null, _images);
            services.ModuleOpened += OpenModule;
            services.LogoutRequested += () => RunAuthVoid("Logout", _sdk.Authentication.LogoutAsync());
            _nav.SetRoot(services);
        }

        private void OpenModule(ServiceMeta m)
        {
            // M1: no per-service IServiceView yet -> "coming soon". M2+ resolves a real view by m.Id.
            _nav.Push(new ComingSoonView(m, () => _nav.Back()));
        }

        private async void RunAuth<T>(string label, AsyncOperation<RestApiResult<T>> op)
        {
            if (op == null)
            {
                return;
            }
            try
            {
                await op.Task();
            }
            catch (Exception e)
            {
                _toasts.Fail(label + " · " + e.Message);
                return;
            }

            if (_sdk.Authentication.IsAuth)
            {
                _popup?.Close();
                _toasts.Ok(label + " · signed in");
                // navigation handled by Authentication.OnLogin
            }
            else
            {
                _toasts.Fail(label + " · " + ErrText(op.Result));
            }
        }

        private async void RunAuthVoid(string label, AsyncOperation<RestApiResult> op)
        {
            if (op == null)
            {
                return;
            }
            await op.Task();
            if (!_sdk.Authentication.IsAuth)
            {
                _toasts.Info("Signed out");
                ShowAuth();
            }
            else
            {
                _toasts.Fail(label + " failed");
            }
        }

        private static string ErrText(RestApiResult r)
        {
            var e = r?.Error;
            if (e == null)
            {
                return "failed";
            }
            return string.IsNullOrEmpty(e.Message) ? e.Type.ToString() : e.Message;
        }

        private static VisualElement New(string cls)
        {
            var e = new VisualElement();
            if (!string.IsNullOrEmpty(cls))
            {
                e.AddToClassList(cls);
            }
            return e;
        }
    }
}
