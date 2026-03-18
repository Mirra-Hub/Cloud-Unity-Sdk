using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MirraCloud.Core.Realtime.Abstractions;
using MirraCloud.Core.Realtime.Protocol;
using MirraCloud.Json;

namespace MirraCloud.Core.Realtime.Connection
{
    internal sealed class RealtimeRequestTracker : IRealtimeRequestTracker
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<RealtimeCommandResult>> _pending = new();

        public async Task<RealtimeCommandResult> RegisterAndWaitAsync(string requestId, int timeoutMs)
        {
            var tcs = new TaskCompletionSource<RealtimeCommandResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (_pending.TryAdd(requestId, tcs) == false)
            {
                return RealtimeCommandResult.Error(requestId, "duplicate_request", "Request already exists.");
            }

            using var timeoutCts = new CancellationTokenSource(Math.Max(1000, timeoutMs));
            using var reg = timeoutCts.Token.Register(() =>
            {
                if (_pending.TryRemove(requestId, out var pending))
                {
                    pending.TrySetResult(RealtimeCommandResult.Error(requestId, "timeout", "Realtime command timeout."));
                }
            });

            return await tcs.Task;
        }

        public void CompleteAck(string requestId, JsonValue payload)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return;
            }

            if (_pending.TryRemove(requestId, out var tcs))
            {
                tcs.TrySetResult(RealtimeCommandResult.Success(requestId, payload));
            }
        }

        public void CompleteError(string requestId, string code, string message)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return;
            }

            if (_pending.TryRemove(requestId, out var tcs))
            {
                tcs.TrySetResult(RealtimeCommandResult.Error(requestId, code, message));
            }
        }

        public void FailAll(string code, string message)
        {
            foreach (var pair in _pending)
            {
                if (_pending.TryRemove(pair.Key, out var tcs))
                {
                    tcs.TrySetResult(RealtimeCommandResult.Error(pair.Key, code, message));
                }
            }
        }
    }
}
