using System;
using MirraCloud.Json;

namespace MirraCloud
{
    public class RestApiOperation : BaseRestApiOperation, IRestApiOperation
    {
        public event Action<RestApiOperation> OnCompleted;

        private event Action<RestApiOperation> CompletedCallback;
        
        public RestApiOperation(IJsonService jsonService) : base(jsonService)
        {
        }

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
        private event Action<RestApiOperation<T>> CompletedCallback;

        public RestApiOperation(IJsonService jsonService) : base(jsonService)
        {
        }

        public override void Complete()
        {
            if (ExtractDataCallback != null)
            {
                Value = ExtractDataCallback.Invoke(this);
            }
            else
            {
                Value = GetData<T>();
            }
            
            CompletedCallback?.Invoke(this);
            
            base.Complete();

            OnCompleted?.Invoke(this);
        }
        
        public override void Dispose()
        {
            OnCompleted = null;
            ExtractDataCallback = null;
        }
        
        public void UseCompletedCallback(Action<RestApiOperation<T>> callback)
        {
            CompletedCallback += callback;
        }

        public void UseExtractDataCallback(Func<RestApiOperation<T>, T> callback)
        {
            ExtractDataCallback = callback;
        }
    }
}