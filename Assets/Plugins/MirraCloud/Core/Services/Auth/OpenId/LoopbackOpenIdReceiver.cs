#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plugins.MirraCloud.Core.General.AsyncOperations;

namespace MirraCloud.Core.Auth.OpenId
{
    internal sealed class LoopbackOpenIdReceiver : IOpenIdCallbackReceiver
    {
        private readonly SynchronizationContext _syncContext;
        private readonly HttpListener _listener;
        private readonly string _successUrl;
        private readonly AsyncOperation<string> _keyOp;
        private bool _disposed;

        public string SuccessUrl => _successUrl;

        private LoopbackOpenIdReceiver(HttpListener listener, string successUrl, SynchronizationContext syncContext)
        {
            _listener = listener;
            _successUrl = successUrl;
            _syncContext = syncContext;
            _keyOp = new AsyncOperation<string>();
        }

        public static bool TryCreate(int port, out IOpenIdCallbackReceiver receiver, out string error)
        {
            receiver = null;
            error = null;

            try
            {
                var actualPort = port > 0 ? port : GetFreeTcpPort();
                var prefix = $"http://127.0.0.1:{actualPort}/mirra-openid/";

                var listener = new HttpListener();
                listener.Prefixes.Add(prefix);
                listener.Start();

                var syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
                var impl = new LoopbackOpenIdReceiver(listener, prefix, syncContext);
                impl.BeginWaitForCallback();

                receiver = impl;
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to start loopback receiver: {ex.Message}";
                return false;
            }
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

            try
            {
                if (_listener.IsListening)
                {
                    _listener.Stop();
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                _listener.Close();
            }
            catch
            {
                // ignored
            }
        }

        private void BeginWaitForCallback()
        {
            Task.Run(async () =>
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    var callbackUrl = context.Request.Url?.ToString();

                    var ok = OpenIdCallbackUrlParser.TryGetOpenIdKey(callbackUrl, out var key);
                    await WriteHtmlAsync(context.Response, ok);

                    _syncContext.Post(_ => _keyOp.Complete(ok ? key : null), null);
                }
                catch (Exception)
                {
                    _syncContext.Post(_ => _keyOp.Complete(null), null);
                }
                finally
                {
                    Dispose();
                }
            });
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static async Task WriteHtmlAsync(HttpListenerResponse response, bool isSuccess)
        {
            const string okHtml = "<html><body><h3>MirraCloud login complete</h3><p>You can close this window.</p></body></html>";
            const string failHtml = "<html><body><h3>MirraCloud login failed</h3><p>Invalid callback.</p></body></html>";

            var html = isSuccess ? okHtml : failHtml;
            var bytes = Encoding.UTF8.GetBytes(html);

            response.StatusCode = 200;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = bytes.Length;

            try
            {
                await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch
            {
                // ignored
            }
            finally
            {
                try { response.OutputStream.Close(); } catch { /* ignored */ }
                try { response.Close(); } catch { /* ignored */ }
            }
        }
    }
}
#endif

