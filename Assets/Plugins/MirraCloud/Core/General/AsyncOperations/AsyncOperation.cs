using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Plugins.MirraCloud.Core.General.AsyncOperations
{
    public interface IBaseAsyncOperation
    {
        bool IsDone { get; }
        Task Task { get; }
    }

    public interface IAsyncOperation<out T> : IBaseAsyncOperation
    {
        T Result { get; }
        event Action<IAsyncOperation<T>> OnCompleted;
    }

    public sealed class AsyncOperation : CustomYieldInstruction, IBaseAsyncOperation
    {
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public bool IsDone => _tcs.Task.IsCompleted;
        public Task Task => _tcs.Task;

        public override bool keepWaiting => IsDone == false;

        public event Action<AsyncOperation> OnCompleted;

        public void Complete()
        {
            if (_tcs.TrySetResult(true))
            {
                OnCompleted?.Invoke(this);
            }
        }

        public static AsyncOperation Completed()
        {
            var op = new AsyncOperation();
            op.Complete();
            return op;
        }
    }

    public sealed class AsyncOperation<T> : CustomYieldInstruction, IAsyncOperation<T>
    {
        private readonly TaskCompletionSource<T> _tcs = new TaskCompletionSource<T>();
        private T _result;

        public bool IsDone => _tcs.Task.IsCompleted;
        public Task Task => _tcs.Task;

        public Task<T> TaskWithResult => _tcs.Task;

        public T Result => _result;
        public override bool keepWaiting => IsDone == false;

        public event Action<IAsyncOperation<T>> OnCompleted;

        public void Complete(T result)
        {
            _result = result;
            if (_tcs.TrySetResult(result))
            {
                OnCompleted?.Invoke(this);
            }
        }

        public static AsyncOperation<T> Completed(T result)
        {
            var op = new AsyncOperation<T>();
            op.Complete(result);
            return op;
        }
    }
}

