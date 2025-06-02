using System;

namespace MirraCloud
{
    public class RestApiOperation : BaseRestApiOperation, IRestApiOperation
    {
        public event Action<RestApiOperation> OnCompleted;

        private event Action<RestApiOperation> CompletedCallback;

        public void UseCompletedCallback(Action<RestApiOperation> callback)
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

    public class RestApiOperation<T> : BaseRestApiOperation,  IRestApiOperation<T>
    {
        public T Value { get; private set; }


        private event Func<RestApiOperation<T>, T> ExtractDataCallback;

        public event Action<RestApiOperation<T>> OnCompleted;

        public override void Complete()
        {
            if (ExtractDataCallback != null)
            {
                Value = ExtractDataCallback.Invoke(this);
            }
            
            base.Complete();

            OnCompleted?.Invoke(this);
        }
        
        public override void Dispose()
        {
            OnCompleted = null;
            ExtractDataCallback = null;
        }

        public void UseExtractData(Func<RestApiOperation<T>, T> callback)
        {
            ExtractDataCallback = callback;
        }
    }
}