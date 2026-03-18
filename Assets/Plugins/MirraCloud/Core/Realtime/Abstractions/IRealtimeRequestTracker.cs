using System.Threading.Tasks;
using MirraCloud.Core.Realtime.Protocol;
using MirraCloud.Json;

namespace MirraCloud.Core.Realtime.Abstractions
{
    public interface IRealtimeRequestTracker
    {
        Task<RealtimeCommandResult> RegisterAndWaitAsync(string requestId, int timeoutMs);
        void CompleteAck(string requestId, JsonValue payload);
        void CompleteError(string requestId, string code, string message);
        void FailAll(string code, string message);
    }
}
