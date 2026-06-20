using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// VContainer entry point. Resolves the live SDK from the Root scope, ensures init,
    /// then owns sandbox logic: auth flows, the auth gate, request history, toasts, and
    /// routing control invocations to the SDK.
    /// </summary>
    public sealed class SandboxApp : IStartable
    {
        private readonly IMirraCloudSdk _sdk;
        private readonly UIDocument _document;

        private SandboxView _view;
        private readonly List<OpResult> _history = new List<OpResult>();

        public SandboxApp(IMirraCloudSdk sdk, UIDocument document)
        {
            _sdk = sdk;
            _document = document;
        }

        public void Start()
        {
            if (!_sdk.IsInitialized)
            {
                _sdk.Initialize();
            }

            string project = "", branch = "", platform = "", url = "";
            try
            {
                var cfg = MirraCloud.Configuration.Load();
                if (cfg != null)
                {
                    project = cfg.ProjectId;
                    branch = cfg.BranchId;
                    platform = cfg.AnalyticsPlatformId;
                    url = cfg.Url;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Sandbox] Could not load Configuration: " + e.Message);
            }

            var modules = ModuleCatalog.Build(_sdk);
            _view = new SandboxView(_document.rootVisualElement, modules, project, branch, platform, url);

            _view.GuestLoginRequested += () => RunAuth("Login Guest", _sdk.Authentication.LoginGuestAsync());
            _view.DeviceLoginRequested += () => RunAuth("Login Device", _sdk.Authentication.LoginDeviceAsync(SystemInfo.deviceUniqueIdentifier));
            _view.EmailLoginRequested += (email, pass) => RunAuth("Login Email", _sdk.Authentication.LoginEmailAsync(email, pass));
            _view.LogoutRequested += () => RunAuthVoid("Logout", _sdk.Authentication.LogoutAsync());
            _view.InvokeRequested += OnInvoke;
            _view.HistoryToggleRequested += () => _view.ShowHistory(_history);

            _sdk.Authentication.OnLogin += _ => UpdateAuth();
            _sdk.Authentication.OnSessionExpired += UpdateAuth;
            _sdk.Authentication.OnSessionRefreshed += UpdateAuth;

            _view.ShowHome();
            UpdateAuth();
        }

        private async void RunAuth<T>(string label, AsyncOperation<RestApiResult<T>> op)
        {
            _view.SetBusy(true);
            OpResult r;
            try
            {
                r = await SandboxOps.Run(op);
            }
            catch (Exception e)
            {
                r = new OpResult { Ok = false, Status = "Exception", Body = e.ToString() };
            }
            r.Label = label;
            Record(r);

            bool authed = _sdk.Authentication.IsAuth;
            UpdateAuth();
            _view.Toast(label + (authed ? " · logged in" : (r.Ok ? " · ok" : " · failed")), authed || r.Ok);
            _view.SetBusy(false);
        }

        private async void RunAuthVoid(string label, AsyncOperation<RestApiResult> op)
        {
            _view.SetBusy(true);
            OpResult r;
            try
            {
                r = await SandboxOps.Run(op);
            }
            catch (Exception e)
            {
                r = new OpResult { Ok = false, Status = "Exception", Body = e.ToString() };
            }
            r.Label = label;
            Record(r);
            UpdateAuth();
            _view.Toast(label + " · " + (r.Ok ? "ok" : "failed"), r.Ok);
            _view.SetBusy(false);
        }

        private async void OnInvoke(ControlDescriptor c)
        {
            if (!_sdk.Authentication.IsAuth)
            {
                var gate = new OpResult
                {
                    Ok = false,
                    Label = c.Label,
                    Status = "Login required",
                    Body = "Press “Login Guest” first — this call needs an authenticated session."
                };
                _view.SetOutput(gate);
                _view.Toast("Login required", false);
                return;
            }

            _view.SetBusy(true);
            _view.SetOutputBusy(c.Label);

            OpResult r;
            try
            {
                r = await c.Invoke();
            }
            catch (Exception e)
            {
                r = new OpResult { Ok = false, Status = "Exception", Body = e.ToString() };
            }
            r.Label = c.Label;

            _view.SetOutput(r);
            Record(r);
            _view.Toast(c.Label + " · " + (r.Ok ? "ok" : "failed"), r.Ok);
            _view.SetBusy(false);
        }

        private void Record(OpResult r)
        {
            _history.Insert(0, r);
            if (_history.Count > 50)
            {
                _history.RemoveAt(_history.Count - 1);
            }
        }

        private void UpdateAuth()
        {
            _view.SetAuthed(_sdk.Authentication.IsAuth);
        }
    }
}
