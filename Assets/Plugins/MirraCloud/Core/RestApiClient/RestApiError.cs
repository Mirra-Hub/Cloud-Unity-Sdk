using System;
using UnityEngine.Networking;

namespace MirraCloud.Core
{
    [Serializable]
    public sealed class RestApiError
    {
        public RestApiErrorType Type;
        public string Message;

        public string Method;
        public string Route;
        public string Url;

        public long? HttpStatusCode;
        public UnityWebRequest.Result? NetworkResult;

        public string ResponseBody;

        public static RestApiError Validation(string message)
        {
            return new RestApiError
            {
                Type = RestApiErrorType.Validation,
                Message = message
            };
        }
    }
}

