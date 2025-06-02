using System;

namespace MirraCloud
{
    public interface IRestApiOperation<T> : IBaseRestApiOperation
    {
        public T Value { get; }

        public event Action<RestApiOperation<T>> OnCompleted;
    }

    public interface IRestApiOperation : IBaseRestApiOperation
    {
        public event Action<RestApiOperation> OnCompleted;
    }
}