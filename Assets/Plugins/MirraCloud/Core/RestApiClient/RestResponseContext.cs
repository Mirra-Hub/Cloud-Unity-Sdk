using UnityEngine.Networking;

namespace MirraCloud.Core
{
    /// <summary>
    /// Контекст ответа, передаваемый в response-интерсепторы.
    /// </summary>
    public class RestResponseContext
    {
        public RestRequestConfig Config { get; }
        public UnityWebRequest Request { get; }
        public BaseRestApiOperation Operation { get; }

        /// <summary>
        /// Интерсептор может выставить true, чтобы повторить запрос (если разрешают настройки ретраев).
        /// </summary>
        public bool RetryRequested { get; set; }

        public RestResponseContext(RestRequestConfig config, UnityWebRequest request, BaseRestApiOperation operation)
        {
            Config = config;
            Request = request;
            Operation = operation;
        }
    }
}

