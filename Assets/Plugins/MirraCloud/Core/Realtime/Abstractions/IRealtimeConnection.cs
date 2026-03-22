using System;
using MirraCloud.Core.Realtime.Connection;
using MirraCloud.Core.Realtime.Protocol;
using Plugins.MirraCloud.Core.General.AsyncOperations;

namespace MirraCloud.Core.Realtime.Abstractions
{
    public interface IRealtimeConnection
    {
        bool IsConnected { get; }
        RealtimeConnectionState State { get; }

        event Action<RealtimeConnectionState> OnStateChanged;
        event Action<RealtimeEvent> OnEvent;
        event Action<RealtimeError> OnError;

        AsyncOperation<RealtimeCommandResult> Connect(string wsUrl);
        AsyncOperation<RealtimeCommandResult> Disconnect();
        AsyncOperation<RealtimeCommandResult> SendCommand(string name, object payload, int timeoutMs = 10000);

        void Initialize();
        void Dispose();
        void SubscribeEvent<T>(string eventName, Action<T, object> callback) where T : IRealtimeEvent;
        void UnsubscribeEvent<T>(string eventName, Action<T, object> callback) where T : IRealtimeEvent;
    }
}
