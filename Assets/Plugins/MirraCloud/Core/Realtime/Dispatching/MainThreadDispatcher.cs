using System;
using System.Threading;

namespace MirraCloud.Core.Realtime.Dispatching
{
    internal sealed class MainThreadDispatcher
    {
        private readonly SynchronizationContext _syncContext;

        public MainThreadDispatcher(SynchronizationContext syncContext)
        {
            _syncContext = syncContext;
        }

        public void Post(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (_syncContext == null)
            {
                action.Invoke();
                return;
            }

            _syncContext.Post(_ => action.Invoke(), null);
        }
    }
}
