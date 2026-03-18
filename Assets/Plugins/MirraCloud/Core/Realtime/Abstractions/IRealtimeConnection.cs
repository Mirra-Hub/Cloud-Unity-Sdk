using System;
using System.Threading;
using System.Threading.Tasks;
using MirraCloud.Core.Realtime.Connection;
using MirraCloud.Core.Realtime.Protocol;

namespace MirraCloud.Core.Realtime.Abstractions
{
    public interface IRealtimeConnection
    {
        bool IsConnected { get; }
        RealtimeConnectionState State { get; }

        event Action<RealtimeConnectionState> OnStateChanged;
        event Action<RealtimeEvent> OnEvent;
        event Action<RealtimeError> OnError;

        Task ConnectAsync(string wsUrl, CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task<RealtimeCommandResult> SendCommandAsync(string name, object payload, int timeoutMs = 10000,
            CancellationToken cancellationToken = default);

        void Initialize();
        void Dispose();
        void SubscribeEvent<T>(string eventName, Action<T, object> callback) where T: IRealtimeEvent;
        void UnsubscribeEvent<T>(string eventName, Action<T, object> callback) where T: IRealtimeEvent;
    }
}
