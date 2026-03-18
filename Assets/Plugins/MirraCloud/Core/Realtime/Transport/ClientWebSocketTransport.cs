using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MirraCloud.Core.Realtime.Abstractions;

namespace MirraCloud.Core.Realtime.Transport
{
    internal sealed class ClientWebSocketTransport : IRealtimeTransport
    {
        private ClientWebSocket _socket;
        private CancellationTokenSource _receiveCts;
        private Task _receiveLoop;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private readonly object _closeSync = new object();
        private bool _closedNotified;

        public bool IsConnected => _socket != null && _socket.State == WebSocketState.Open;

        public event Action<string> OnMessage;
        public event Action OnClosed;
        public event Action<Exception> OnError;

        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (_socket != null)
            {
                await CloseAsync(cancellationToken);
            }

            lock (_closeSync)
            {
                _closedNotified = false;
            }

            _socket = new ClientWebSocket();
            await _socket.ConnectAsync(uri, cancellationToken);

            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _receiveLoop = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token));
        }

        public async Task SendAsync(string message, CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("WebSocket is not connected.");
            }

            var bytes = Encoding.UTF8.GetBytes(message);
            await _sendLock.WaitAsync(cancellationToken);
            try
            {
                await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            var receiveCts = _receiveCts;
            var socket = _socket;
            var receiveLoop = _receiveLoop;

            _receiveCts = null;
            _receiveLoop = null;
            _socket = null;

            try
            {
                receiveCts?.Cancel();
            }
            catch
            {
            }
            finally
            {
                receiveCts?.Dispose();
            }

            if (socket == null)
            {
                NotifyClosedOnce();
                return;
            }

            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closed", cancellationToken);
                }
                catch
                {
                }
            }

            socket.Dispose();

            if (receiveLoop != null)
            {
                try
                {
                    await receiveLoop;
                }
                catch
                {
                }
            }

            NotifyClosedOnce();
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[16 * 1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var socket = _socket;
                    if (socket == null || socket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    var message = await ReceiveMessageAsync(socket, buffer, cancellationToken);
                    if (message == null)
                    {
                        break;
                    }

                    OnMessage?.Invoke(message);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
            finally
            {
                NotifyClosedOnce();
            }
        }

        private async Task<string> ReceiveMessageAsync(ClientWebSocket socket, byte[] buffer, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream();

            while (true)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                ms.Write(buffer, 0, result.Count);

                if (result.EndOfMessage)
                {
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        private void NotifyClosedOnce()
        {
            lock (_closeSync)
            {
                if (_closedNotified)
                {
                    return;
                }

                _closedNotified = true;
            }

            OnClosed?.Invoke();
        }
    }
}
