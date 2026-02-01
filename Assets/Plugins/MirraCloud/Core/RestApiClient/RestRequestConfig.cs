using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace MirraCloud.Core
{
    public class RestRequestConfig
    {
        public string Route;
        public string Method;
        public object Body;
        public byte[] SerializedBody;
        public Dictionary<string, string> Headers;
        public List<IMultipartFormSection> MultipartFormSections;
        public DownloadHandler DownloadHandler;
        public Func<string, DownloadHandler> DownloadHandlerFactory;
        public UploadHandler UploadHandler;
        public int? TimeoutMs;
        public int? RedirectLimit;
        public bool FollowRedirect;
        public int MaxRedirects = 5;
        public long[] RedirectHttpStatusCodes;
        public bool NoAuthOnRedirect;
        public bool StripHeadersOnRedirect;
        public long[] AllowedHttpStatusCodes;
        public int MaxRetries = 1;
        public int RetryCount;
        public bool AuthRetryAttempted;
        public bool DisableRetry;
        public bool NoAuth;

        internal string Url;
    }
}
