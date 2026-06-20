using System;
using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts.Core;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// VContainer entry point for the sandbox. Resolves the live SDK + example AuthService
    /// from the persistent Root scope, ensures the SDK is initialized, then drives the view.
    /// </summary>
    public sealed class SandboxApp : IStartable
    {
        private readonly IMirraCloudSdk _sdk;
        private readonly AuthService _auth;
        private readonly UIDocument _document;

        private SandboxView _view;

        public SandboxApp(IMirraCloudSdk sdk, AuthService auth, UIDocument document)
        {
            _sdk = sdk;
            _auth = auth;
            _document = document;
        }

        public void Start()
        {
            // Works standalone (open MC_Sandbox + Play) or via the example bootstrap.
            if (!_sdk.IsInitialized)
            {
                _sdk.Initialize();
            }

            var modules = ModuleCatalog.Build(_sdk);
            _view = new SandboxView(_document.rootVisualElement, modules);
            _view.LoginRequested += OnLogin;
            _view.InvokeRequested += OnInvoke;
            _view.ShowHome();
            UpdateStatus();
        }

        private async void OnLogin()
        {
            _view.SetBusy(true);
            _view.SetStatus("logging in…", false);
            try
            {
                await _auth.LoginGuest();
            }
            catch (Exception e)
            {
                Debug.LogError("[Sandbox] Guest login failed: " + e);
            }
            UpdateStatus();
            _view.SetBusy(false);
        }

        private async void OnInvoke(ControlDescriptor c)
        {
            if (!_sdk.Authentication.IsAuth)
            {
                _view.SetOutput(new OpResult
                {
                    Ok = false,
                    Status = "Login required",
                    Body = "Press “Login Guest” first — this call needs an authenticated session."
                });
                return;
            }

            _view.SetBusy(true);
            _view.SetOutputBusy(c.Label);

            OpResult result;
            try
            {
                result = await c.Invoke();
            }
            catch (Exception e)
            {
                result = new OpResult { Ok = false, Status = "Exception", Body = e.ToString() };
            }

            _view.SetOutput(result);
            _view.SetBusy(false);
        }

        private void UpdateStatus()
        {
            bool authed = _sdk.Authentication.IsAuth;
            _view.SetStatus(authed ? "logged in" : "not logged in", authed);
        }
    }
}
