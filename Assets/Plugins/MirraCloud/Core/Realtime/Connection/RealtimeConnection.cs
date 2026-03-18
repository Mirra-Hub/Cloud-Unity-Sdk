using System;
using System.Threading;
using System.Threading.Tasks;
using MirraCloud.Core.Logger;
using MirraCloud.Core.Realtime.Abstractions;
using MirraCloud.Core.Realtime.Dispatching;
using MirraCloud.Core.Realtime.Protocol;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.LifeCycle;

namespace MirraCloud.Core.Realtime.Connection
{
    public interface IRealtimeEvent
    {
        
    }
    
    internal sealed class RealtimeConnection : IRealtimeConnection
    {
        private readonly IRealtimeTransport _transport;
        private readonly IRealtimeSerializer _serializer;
        private readonly IRealtimeRequestTracker _requestTracker;
        private readonly MainThreadDispatcher _dispatcher;
        private readonly ILogger _logger;

        public bool IsConnected => _transport.IsConnected;
        public RealtimeConnectionState State { get; private set; } = RealtimeConnectionState.Disconnected;

        public event Action<RealtimeConnectionState> OnStateChanged;
        public event Action<RealtimeEvent> OnEvent;
        public event Action<RealtimeError> OnError;

        public RealtimeConnection(
            IRealtimeTransport transport,
            IRealtimeSerializer serializer,
            IRealtimeRequestTracker requestTracker,
            MainThreadDispatcher dispatcher,
            ILogger logger)
        {
            _transport = transport;
            _serializer = serializer;
            _requestTracker = requestTracker;
            _dispatcher = dispatcher;
            _logger = logger;
        }
        
        public void Initialize()
        {
            _transport.OnMessage += HandleTransportMessage;
            _transport.OnClosed += HandleTransportClosed;
            _transport.OnError += HandleTransportError;
        }

        public void Dispose()
        {
            _transport.OnMessage -= HandleTransportMessage;
            _transport.OnClosed -= HandleTransportClosed;
            _transport.OnError -= HandleTransportError;
        }

        public async Task ConnectAsync(string wsUrl, CancellationToken cancellationToken = default)
        {
            if (State is RealtimeConnectionState.Connected or RealtimeConnectionState.Connecting)
            {
                return;
            }

            SetState(RealtimeConnectionState.Connecting);
            try
            {
                await _transport.ConnectAsync(new Uri(wsUrl), cancellationToken);
                SetState(RealtimeConnectionState.Connected);
            }
            catch
            {
                SetState(RealtimeConnectionState.Disconnected);
                throw;
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (State == RealtimeConnectionState.Disconnected)
            {
                return;
            }

            await _transport.CloseAsync(cancellationToken);
            _requestTracker.FailAll("connection_closed", "Realtime connection closed.");
            SetState(RealtimeConnectionState.Disconnected);
        }

        public async Task<RealtimeCommandResult> SendCommandAsync(string name, object payload, int timeoutMs = 10000,
            CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
            {
                return RealtimeCommandResult.Error(string.Empty, "not_connected", "Realtime connection is not established.");
            }

            var requestId = Guid.NewGuid().ToString("N");
            var waitTask = _requestTracker.RegisterAndWaitAsync(requestId, timeoutMs);

            var command = new RealtimeCommand
            {
                RequestId = requestId,
                Name = name,
                Payload = payload is JsonValue jsonValue
                    ? jsonValue
                    : _serializer.Deserialize<JsonValue>(_serializer.Serialize(payload))
            };

            var json = _serializer.Serialize(command);
            try
            {
                await _transport.SendAsync(json, cancellationToken);
            }
            catch (Exception ex)
            {
                _requestTracker.CompleteError(requestId, "send_failed", ex.Message);
                return RealtimeCommandResult.Error(requestId, "send_failed", ex.Message);
            }

            return await waitTask;
        }

        private void HandleTransportMessage(string json)
        {
            RealtimeEnvelope envelope;

            try
            {
                envelope = _serializer.Deserialize<RealtimeEnvelope>(json);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to parse realtime envelope: {ex.Message}");
                return;
            }

            var kind = envelope?.Kind ?? string.Empty;

            if (string.Equals(kind, "ack", StringComparison.OrdinalIgnoreCase))
            {
                _requestTracker.CompleteAck(envelope.RequestId, envelope.Payload);
                return;
            }

            if (string.Equals(kind, "error", StringComparison.OrdinalIgnoreCase))
            {
                _requestTracker.CompleteError(envelope.RequestId, envelope.Code, envelope.Message);
                var realtimeError = new RealtimeError
                {
                    RequestId = envelope.RequestId,
                    Code = envelope.Code,
                    Message = envelope.Message
                };
                _dispatcher.Post(() => OnError?.Invoke(realtimeError));
                return;
            }

            if (string.Equals(kind, "event", StringComparison.OrdinalIgnoreCase))
            {
                var realtimeEvent = new RealtimeEvent
                {
                    Name = envelope.Name,
                    ChannelId = envelope.ChannelId,
                    Payload = envelope.Payload
                };
                _dispatcher.Post(() => OnEvent?.Invoke(realtimeEvent));
            }
        }

        // TODO: add IEvent interface
        // при регистрации сервиса вызываем subscribe с конкретными ивентами котоорые нужны для конкретного сервиса
        
        public void SubscribeEvent<T>(string eventName, Action<T, object> callback) where T: IRealtimeEvent
        {
            
        }
        
        public  void UnsubscribeEvent<T>(string eventName, Action<T, object> callback) where T: IRealtimeEvent
        {
            
        }
        
        private void HandleTransportClosed()
        {
            _requestTracker.FailAll("connection_closed", "Realtime connection closed.");
            SetState(RealtimeConnectionState.Disconnected);
        }

        private void HandleTransportError(Exception ex)
        {
            _logger.Error($"Realtime transport error: {ex.Message}");
            var realtimeError = new RealtimeError
            {
                Code = "transport_error",
                Message = ex.Message
            };

            _dispatcher.Post(() => OnError?.Invoke(realtimeError));
        }

        private void SetState(RealtimeConnectionState state)
        {
            if (State == state)
            {
                return;
            }

            State = state;
            _dispatcher.Post(() => OnStateChanged?.Invoke(state));
        }
    }
}
