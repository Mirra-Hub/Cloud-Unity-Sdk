using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MirraCloud.Json;
using UnityEngine;
using UnityEngine.Networking;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace MirraCloud.Core
{
    public class RestApiClient
    {
        public string BaseUrl { get; private set; }

        private readonly CoroutineRunner _coroutineRunner;
        public readonly IJsonService JsonService;
        private readonly ILogger _logger;

        private readonly List<Func<RestRequestConfig, RestRequestConfig>> _requestInterceptors = new();
        private readonly List<Func<RestResponseContext, IEnumerator>> _responseInterceptors = new();
        
        public RestApiClient(RestApiClientOptions options, CoroutineRunner coroutineRunner, IJsonService jsonService, ILogger logger)
        {
            BaseUrl = options.BaseUrl?.TrimEnd('/');
            _coroutineRunner = coroutineRunner;
            JsonService = jsonService;
            _logger = logger;
        }

        #region Interceptors
        public int UseRequestInterceptor(Func<RestRequestConfig, RestRequestConfig> interceptor)
        {
            _requestInterceptors.Add(interceptor);
            return _requestInterceptors.Count - 1;
        }

        public int UseResponseInterceptor(Func<RestResponseContext, IEnumerator> interceptor)
        {
            _responseInterceptors.Add(interceptor);
            return _responseInterceptors.Count - 1;
        }

        public void EjectRequestInterceptor(int id)
        {
            if (id >= 0 && id < _requestInterceptors.Count)
            {
                _requestInterceptors[id] = null;
            }
        }

        public void EjectResponseInterceptor(int id)
        {
            if (id >= 0 && id < _responseInterceptors.Count)
            {
                _responseInterceptors[id] = null;
            }
        }
        #endregion
        
        #region Public API
        public RestApiOperation Get(string route, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbGET, null, config);
            return SendRequest(finalConfig);
        }
        
        public RestApiOperation<T> Get<T>(string route, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbGET, null, config);
            return SendRequest<T>(finalConfig);
        }

        public RestApiOperation Post(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPOST, body, config);
            return SendRequest(finalConfig);
        }
        
        public RestApiOperation<T> Post<T>(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPOST, body, config);
            return SendRequest<T>(finalConfig);
        }

        public RestApiOperation Put(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPUT, body, config);
            return SendRequest(finalConfig);
        }
        
        public RestApiOperation<T> Put<T>(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPUT, body, config);
            return SendRequest<T>(finalConfig);
        }
        
        public RestApiOperation<T> Patch<T>(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, "PATCH", body, config);
            return SendRequest<T>(finalConfig);
        }
 
        public RestApiOperation Patch(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, "PATCH", body, config);
            return SendRequest(finalConfig);
        }

        public RestApiOperation PatchMultipart(string route, List<IMultipartFormSection> multipartFormSections, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, "PATCH", null, config);
            finalConfig.MultipartFormSections = multipartFormSections;
            return SendRequest(finalConfig);
        }
        
        public RestApiOperation Delete(string route, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbDELETE, null, config);
            return SendRequest(finalConfig);
        }
        
        public RestApiOperation<T> Delete<T>(string route, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbDELETE, null, config);
            return SendRequest<T>(finalConfig);
        }
        #endregion
        
        private RestApiOperation SendRequest(RestRequestConfig config)
        {
            RestApiOperation response = new RestApiOperation(JsonService);
            _coroutineRunner.StartCoroutine(SendRequestInternal(config, response));
            return response;
        }
        
        private RestApiOperation<T> SendRequest<T>(RestRequestConfig config)
        {
            RestApiOperation<T> response = new RestApiOperation<T>(JsonService);
            _coroutineRunner.StartCoroutine(SendRequestInternal(config, response));
            return response;
        }

        private IEnumerator SendRequestInternal(RestRequestConfig config, BaseRestApiOperation operation)
        {
            var preparedConfig = PrepareConfig(config);

            foreach (var interceptor in _requestInterceptors)
            {
                if (interceptor == null) continue;
                var updated = interceptor.Invoke(preparedConfig);
                if (updated != null)
                {
                    preparedConfig = updated;
                }
            }

            UnityWebRequest request = BuildUnityWebRequest(preparedConfig);
            operation.Initialize(request);

            yield return request.SendWebRequest();

            var responseContext = new RestResponseContext(preparedConfig, request, operation);

            foreach (var interceptor in _responseInterceptors)
            {
                if (interceptor == null) continue;
                yield return interceptor.Invoke(responseContext);
            }

            if (responseContext.RetryRequested && preparedConfig.DisableRetry == false && preparedConfig.RetryCount < preparedConfig.MaxRetries)
            {
                preparedConfig.RetryCount++;
                yield return SendRequestInternal(preparedConfig, operation);
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                _logger.Error($"{request.error} {request.downloadHandler?.text}");
            }

            operation.Complete();
        }

        private RestRequestConfig BuildConfig(string route, string method, object body, RestRequestConfig config)
        {
            var cfg = config ?? new RestRequestConfig();
            cfg.Route = route;
            cfg.Method = method;
            if (body != null)
            {
                cfg.Body = body;
            }
            if (cfg.DownloadHandler == null)
            {
                cfg.DownloadHandler = new DownloadHandlerBuffer();
            }
            if (cfg.MaxRetries <= 0)
            {
                cfg.MaxRetries = 1;
            }
            return cfg;
        }

        private RestRequestConfig PrepareConfig(RestRequestConfig config)
        {
            var cfg = new RestRequestConfig
            {
                Route = config.Route,
                Method = config.Method,
                Body = config.Body,
                SerializedBody = config.SerializedBody,
                Headers = config.Headers != null ? new Dictionary<string, string>(config.Headers) : null,
                MultipartFormSections = config.MultipartFormSections != null ? new List<IMultipartFormSection>(config.MultipartFormSections) : null,
                DownloadHandler = config.DownloadHandler,
                UploadHandler = config.UploadHandler,
                TimeoutMs = config.TimeoutMs,
                MaxRetries = config.MaxRetries,
                RetryCount = config.RetryCount,
                DisableRetry = config.DisableRetry,
                NoAuth = config.NoAuth
            };

            cfg.Url = GetUrl(cfg.Route);

            if (cfg.SerializedBody == null && cfg.Body != null)
            {
                var bodyJson = JsonService.ToJson(cfg.Body);
                cfg.SerializedBody = Encoding.UTF8.GetBytes(bodyJson);
            }

            return cfg;
        }

        private UnityWebRequest BuildUnityWebRequest(RestRequestConfig config)
        {
            _logger.Log($"Send {config.Method} request: {config.Url}");
            UnityWebRequest request;
            if (config.MultipartFormSections != null)
            {
                request = UnityWebRequest.Post(config.Url, config.MultipartFormSections);
                request.method = config.Method;
            }
            else
            {
                request = new UnityWebRequest(config.Url, config.Method);
            }

            if (config.SerializedBody != null && config.SerializedBody.Length > 0)
            {
                request.uploadHandler = new UploadHandlerRaw(config.SerializedBody);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            if (config.DownloadHandler == null)
            {
                config.DownloadHandler = new DownloadHandlerBuffer();
            }

            request.downloadHandler = config.DownloadHandler;

            if (config.UploadHandler != null)
            {
                request.uploadHandler = config.UploadHandler;
            }

            if (config.TimeoutMs.HasValue)
            {
                request.timeout = Mathf.CeilToInt(config.TimeoutMs.Value / 1000f);
            }

            if (config.Headers != null)
            {
                foreach (var header in config.Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            return request;
        }

        public string GetUrl(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                return BaseUrl;
            }
            if (route.StartsWith("http"))
            {
                return route;
            }
            return $"{BaseUrl}{route}";
        }
    }
}
