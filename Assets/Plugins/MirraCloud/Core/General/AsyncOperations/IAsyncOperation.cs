using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Plugins.MirraCloud.Core.General.AsyncOperations
{
    public interface IBaseAsyncOperation : IEnumerator
    {
        public bool IsDone { get; }
        public Task Task { get; }
    }
    
    public interface IAsyncOperation : IBaseAsyncOperation
    {
        public event Action<IAsyncOperation> OnCompleted;
    }
    
    public interface IAsyncOperation<T> : IBaseAsyncOperation where T : IAsyncValueHandler
    {
        public T Value { get; }
        public event Action<IAsyncOperation<T>> OnCompleted;
    }

    public abstract class BaseAsyncOperation : CustomYieldInstruction, IBaseAsyncOperation
    {
        public bool IsDone { get; private set; }
        
        public override bool keepWaiting => IsDone == false;
        public Task Task => TaskWait();
        
        public virtual void Complete()
        {
            IsDone = true;
        }

        public virtual void Dispose()
        {
        }

        private async Task TaskWait()
        {
            while (IsDone == false)
            {
                await Task.Yield();
            }
        }
    }
    
    public class AsyncOperation : BaseAsyncOperation, IAsyncOperation
    {
        public event Action<IAsyncOperation> OnCompleted;

        private event Action<AsyncOperation> CompletedCallback;

        public void UseCompletedCallback(Action<AsyncOperation> callback)
        {
            CompletedCallback += callback;
        }

        public override void Complete()
        {
            CompletedCallback?.Invoke(this);
            
            base.Complete();
            
            OnCompleted?.Invoke(this);
        }

        public override void Dispose()
        {
            OnCompleted = null;
            CompletedCallback = null;
        }
    }

    public interface IAsyncValueHandler
    {
        public void Complete();
    }
    
    public class AsyncOperation<T> : BaseAsyncOperation,  IAsyncOperation<T> where T: IAsyncValueHandler
    {
        public T Value { get; private set; }
        
        private event Func<IAsyncOperation<T>, T> ExtractDataCallback;

        public event Action<IAsyncOperation<T>> OnCompleted;
        private event Action<IAsyncOperation<T>> CompletedCallback;

        public override void Complete()
        {
            Value.Complete();
            
            CompletedCallback?.Invoke(this);
            
            base.Complete();

            OnCompleted?.Invoke(this);
        }
        
        public override void Dispose()
        {
            OnCompleted = null;
            ExtractDataCallback = null;
        }
        
        public void UseCompletedCallback(Action<IAsyncOperation<T>> callback)
        {
            CompletedCallback += callback;
        }
    }
}