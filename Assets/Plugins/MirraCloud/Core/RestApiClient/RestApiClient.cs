using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.AsyncOperations;
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
        private ISessionRefresher _sessionRefresher;
        private static readonly long[] DefaultRedirectHttpStatusCodes = { 301, 302, 303, 307, 308 };
        
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

        public void EjectRequestInterceptor(int id)
        {
            if (id >= 0 && id < _requestInterceptors.Count)
            {
                _requestInterceptors[id] = null;
            }
        }
        #endregion

        public void SetSessionRefresher(ISessionRefresher sessionRefresher)
        {
            _sessionRefresher = sessionRefresher;
        }
        
        #region Public API
        public AsyncOperation<RestApiResult> GetAsync(string route, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbGET, null, config);
            return SendRequest(finalConfig);
        }
        
        public AsyncOperation<RestApiResult<T>> GetAsync<T>(string route, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbGET, null, config);
            return SendRequest<T>(finalConfig, null);
        }

        public AsyncOperation<RestApiResult<T>> GetAsync<T>(string route, RestRequestConfig config, Func<UnityWebRequest, T> extractData)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbGET, null, config);
            return SendRequest(finalConfig, extractData);
        }

        public AsyncOperation<RestApiResult> PostAsync(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPOST, body, config);
            return SendRequest(finalConfig);
        }
        
        public AsyncOperation<RestApiResult<T>> PostAsync<T>(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPOST, body, config);
            return SendRequest<T>(finalConfig);
        }

        public AsyncOperation<RestApiResult<T>> PostAsync<T>(string route, object body, RestRequestConfig config, Func<UnityWebRequest, T> extractData)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPOST, body, config);
            return SendRequest(finalConfig, extractData);
        }

        public AsyncOperation<RestApiResult> PutAsync(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPUT, body, config);
            return SendRequest(finalConfig);
        }
        
        public AsyncOperation<RestApiResult<T>> PutAsync<T>(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbPUT, body, config);
            return SendRequest<T>(finalConfig);
        }
        
        public AsyncOperation<RestApiResult<T>> PatchAsync<T>(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, "PATCH", body, config);
            return SendRequest<T>(finalConfig);
        }
 
        public AsyncOperation<RestApiResult> PatchAsync(string route, object body = null, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, "PATCH", body, config);
            return SendRequest(finalConfig);
        }

        public AsyncOperation<RestApiResult> PatchMultipartAsync(string route, List<IMultipartFormSection> multipartFormSections, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, "PATCH", null, config);
            finalConfig.MultipartFormSections = multipartFormSections;
            return SendRequest(finalConfig);
        }
        
        public AsyncOperation<RestApiResult> DeleteAsync(string route, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbDELETE, null, config);
            return SendRequest(finalConfig);
        }
        
        public AsyncOperation<RestApiResult<T>> DeleteAsync<T>(string route, RestRequestConfig config = null)
        {
            var finalConfig = BuildConfig(route, UnityWebRequest.kHttpVerbDELETE, null, config);
            return SendRequest<T>(finalConfig);
        }
        #endregion
        
        private AsyncOperation<RestApiResult> SendRequest(RestRequestConfig config)
        {
            var op = new AsyncOperation<RestApiResult>();
            _coroutineRunner.StartCoroutine(SendRequestInternal(config, op));
            return op;
        }
        
        private AsyncOperation<RestApiResult<T>> SendRequest<T>(RestRequestConfig config, Func<UnityWebRequest, T> extractData)
        {
            var op = new AsyncOperation<RestApiResult<T>>();
            _coroutineRunner.StartCoroutine(SendRequestInternal(config, op, extractData));
            return op;
        }

        private AsyncOperation<RestApiResult<T>> SendRequest<T>(RestRequestConfig config)
        {
            return SendRequest<T>(config, null);
        }

        private IEnumerator SendRequestInternal(RestRequestConfig config, AsyncOperation<RestApiResult> operation, int redirectDepth = 0)
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

            if (preparedConfig.DownloadHandler == null && preparedConfig.DownloadHandlerFactory != null)
            {
                preparedConfig.DownloadHandler = preparedConfig.DownloadHandlerFactory.Invoke(preparedConfig.Url);
            }

            var stopwatch = Stopwatch.StartNew();
            UnityWebRequest request = BuildUnityWebRequest(preparedConfig);
            yield return request.SendWebRequest();
            stopwatch.Stop();

            #if UNITY_EDITOR
            var requestBodyForTrace = GetRequestBodyForTrace(preparedConfig);
            #endif

            var responseBody = request.downloadHandler?.text;
            var httpCode = request.responseCode;
            var isHttpSuccess = IsHttpSuccess(httpCode, preparedConfig.AllowedHttpStatusCodes);

            if (preparedConfig.FollowRedirect && IsRedirectStatus(httpCode, preparedConfig.RedirectHttpStatusCodes))
            {
                var redirectLocation = ExtractRedirectLocation(request);
                var redirectUrl = ResolveRedirectUrl(preparedConfig.Url, redirectLocation);

                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    var redirectResult = RestApiResult.Fail(RestApiError.Validation("Redirect location is empty."));
                    redirectResult.Method = preparedConfig.Method;
                    redirectResult.Route = preparedConfig.Route;
                    redirectResult.Url = preparedConfig.Url;
                    redirectResult.HttpStatusCode = httpCode > 0 ? httpCode : null;
                    redirectResult.RetryCount = preparedConfig.RetryCount;
                    redirectResult.DurationMs = stopwatch.ElapsedMilliseconds;
                    redirectResult.ResponseBody = responseBody;

                    #if UNITY_EDITOR
                    Debugging.RestApiTraceBus.Record(preparedConfig, redirectResult, requestBodyForTrace);
                    #endif

                    operation.Complete(redirectResult);
                    yield break;
                }

                if (redirectDepth >= preparedConfig.MaxRedirects)
                {
                    var redirectResult = RestApiResult.Fail(RestApiError.Validation("Redirect limit exceeded."));
                    redirectResult.Method = preparedConfig.Method;
                    redirectResult.Route = preparedConfig.Route;
                    redirectResult.Url = preparedConfig.Url;
                    redirectResult.HttpStatusCode = httpCode > 0 ? httpCode : null;
                    redirectResult.RetryCount = preparedConfig.RetryCount;
                    redirectResult.DurationMs = stopwatch.ElapsedMilliseconds;
                    redirectResult.ResponseBody = responseBody;

                    #if UNITY_EDITOR
                    Debugging.RestApiTraceBus.Record(preparedConfig, redirectResult, requestBodyForTrace);
                    #endif

                    operation.Complete(redirectResult);
                    yield break;
                }

                var redirectConfig = BuildRedirectConfig(preparedConfig, redirectUrl, httpCode);
                yield return SendRequestInternal(redirectConfig, operation, redirectDepth + 1);
                yield break;
            }

            if ((httpCode == 401 || httpCode == 403) && preparedConfig.NoAuth == false && preparedConfig.AuthRetryAttempted == false &&
                _sessionRefresher != null && _sessionRefresher.CanRefresh)
            {
                preparedConfig.AuthRetryAttempted = true;
                var refreshOp = _sessionRefresher.RefreshSessionAsync();
                yield return refreshOp;
                if (refreshOp.Result.IsSuccess)
                {
                    preparedConfig.RetryCount++;
                    yield return SendRequestInternal(preparedConfig, operation, redirectDepth);
                    yield break;
                }
            }

            if (request.result != UnityWebRequest.Result.Success && isHttpSuccess == false && preparedConfig.DisableRetry == false &&
                preparedConfig.RetryCount < preparedConfig.MaxRetries)
            {
                preparedConfig.RetryCount++;
                yield return SendRequestInternal(preparedConfig, operation, redirectDepth);
                yield break;
            }

            RestApiResult result;

            if (isHttpSuccess)
            {
                result = RestApiResult.Success();
            }
            else if (httpCode > 0)
            {
                result = RestApiResult.Fail(new RestApiError
                {
                    Type = RestApiErrorType.Http,
                    Message = request.error,
                    Method = preparedConfig.Method,
                    Route = preparedConfig.Route,
                    Url = preparedConfig.Url,
                    HttpStatusCode = httpCode,
                    NetworkResult = request.result,
                    ResponseBody = responseBody
                });
            }
            else
            {
                result = RestApiResult.Fail(new RestApiError
                {
                    Type = RestApiErrorType.Network,
                    Message = request.error,
                    Method = preparedConfig.Method,
                    Route = preparedConfig.Route,
                    Url = preparedConfig.Url,
                    NetworkResult = request.result,
                    ResponseBody = responseBody
                });
            }

            result.Method = preparedConfig.Method;
            result.Route = preparedConfig.Route;
            result.Url = preparedConfig.Url;
            result.HttpStatusCode = httpCode > 0 ? httpCode : null;
            result.RetryCount = preparedConfig.RetryCount;
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.ResponseBody = responseBody;

            #if UNITY_EDITOR
            Debugging.RestApiTraceBus.Record(preparedConfig, result, requestBodyForTrace);
            #endif

            operation.Complete(result);
        }

        private IEnumerator SendRequestInternal<T>(RestRequestConfig config, AsyncOperation<RestApiResult<T>> operation, Func<UnityWebRequest, T> extractData, int redirectDepth = 0)
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

            if (preparedConfig.DownloadHandler == null && preparedConfig.DownloadHandlerFactory != null)
            {
                preparedConfig.DownloadHandler = preparedConfig.DownloadHandlerFactory.Invoke(preparedConfig.Url);
            }

            var stopwatch = Stopwatch.StartNew();
            UnityWebRequest request = BuildUnityWebRequest(preparedConfig);
            yield return request.SendWebRequest();
            stopwatch.Stop();

            #if UNITY_EDITOR
            var requestBodyForTrace = GetRequestBodyForTrace(preparedConfig);
            #endif

            var responseBody = request.downloadHandler?.text;
            var httpCode = request.responseCode;
            var isHttpSuccess = IsHttpSuccess(httpCode, preparedConfig.AllowedHttpStatusCodes);

            if (preparedConfig.FollowRedirect && IsRedirectStatus(httpCode, preparedConfig.RedirectHttpStatusCodes))
            {
                var redirectLocation = ExtractRedirectLocation(request);
                var redirectUrl = ResolveRedirectUrl(preparedConfig.Url, redirectLocation);

                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    var redirectResult = RestApiResult<T>.Fail(RestApiError.Validation("Redirect location is empty."));
                    redirectResult.Method = preparedConfig.Method;
                    redirectResult.Route = preparedConfig.Route;
                    redirectResult.Url = preparedConfig.Url;
                    redirectResult.HttpStatusCode = httpCode > 0 ? httpCode : null;
                    redirectResult.RetryCount = preparedConfig.RetryCount;
                    redirectResult.DurationMs = stopwatch.ElapsedMilliseconds;
                    redirectResult.ResponseBody = responseBody;

                    #if UNITY_EDITOR
                    Debugging.RestApiTraceBus.Record(preparedConfig, redirectResult, requestBodyForTrace);
                    #endif

                    operation.Complete(redirectResult);
                    yield break;
                }

                if (redirectDepth >= preparedConfig.MaxRedirects)
                {
                    var redirectResult = RestApiResult<T>.Fail(RestApiError.Validation("Redirect limit exceeded."));
                    redirectResult.Method = preparedConfig.Method;
                    redirectResult.Route = preparedConfig.Route;
                    redirectResult.Url = preparedConfig.Url;
                    redirectResult.HttpStatusCode = httpCode > 0 ? httpCode : null;
                    redirectResult.RetryCount = preparedConfig.RetryCount;
                    redirectResult.DurationMs = stopwatch.ElapsedMilliseconds;
                    redirectResult.ResponseBody = responseBody;

                    #if UNITY_EDITOR
                    Debugging.RestApiTraceBus.Record(preparedConfig, redirectResult, requestBodyForTrace);
                    #endif

                    operation.Complete(redirectResult);
                    yield break;
                }

                var redirectConfig = BuildRedirectConfig(preparedConfig, redirectUrl, httpCode);
                yield return SendRequestInternal(redirectConfig, operation, extractData, redirectDepth + 1);
                yield break;
            }

            if ((httpCode == 401 || httpCode == 403) && preparedConfig.NoAuth == false && preparedConfig.AuthRetryAttempted == false &&
                _sessionRefresher != null && _sessionRefresher.CanRefresh)
            {
                preparedConfig.AuthRetryAttempted = true;
                var refreshOp = _sessionRefresher.RefreshSessionAsync();
                yield return refreshOp;
                if (refreshOp.Result.IsSuccess)
                {
                    preparedConfig.RetryCount++;
                    yield return SendRequestInternal(preparedConfig, operation, extractData, redirectDepth);
                    yield break;
                }
            }

            if (request.result != UnityWebRequest.Result.Success && isHttpSuccess == false && preparedConfig.DisableRetry == false &&
                preparedConfig.RetryCount < preparedConfig.MaxRetries)
            {
                preparedConfig.RetryCount++;
                yield return SendRequestInternal(preparedConfig, operation, extractData, redirectDepth);
                yield break;
            }

            RestApiResult<T> result;

            if (isHttpSuccess)
            {
                if (extractData != null)
                {
                    try
                    {
                        var data = extractData.Invoke(request);
                        result = RestApiResult<T>.Success(data);
                    }
                    catch (Exception ex)
                    {
                        result = RestApiResult<T>.Fail(new RestApiError
                        {
                            Type = RestApiErrorType.Deserialize,
                            Message = ex.Message,
                            Method = preparedConfig.Method,
                            Route = preparedConfig.Route,
                            Url = preparedConfig.Url,
                            HttpStatusCode = httpCode,
                            NetworkResult = request.result,
                            ResponseBody = responseBody
                        });
                    }
                }
                else if (string.IsNullOrEmpty(responseBody))
                {
                    result = RestApiResult<T>.Success(default);
                }
                else
                {
                    try
                    {
                        var data = JsonService.FromJson<T>(responseBody);
                        result = RestApiResult<T>.Success(data);
                    }
                    catch (Exception ex)
                    {
                        result = RestApiResult<T>.Fail(new RestApiError
                        {
                            Type = RestApiErrorType.Deserialize,
                            Message = ex.Message,
                            Method = preparedConfig.Method,
                            Route = preparedConfig.Route,
                            Url = preparedConfig.Url,
                            HttpStatusCode = httpCode,
                            NetworkResult = request.result,
                            ResponseBody = responseBody
                        });
                    }
                }
            }
            else if (httpCode > 0)
            {
                result = RestApiResult<T>.Fail(new RestApiError
                {
                    Type = RestApiErrorType.Http,
                    Message = request.error,
                    Method = preparedConfig.Method,
                    Route = preparedConfig.Route,
                    Url = preparedConfig.Url,
                    HttpStatusCode = httpCode,
                    NetworkResult = request.result,
                    ResponseBody = responseBody
                });
            }
            else
            {
                result = RestApiResult<T>.Fail(new RestApiError
                {
                    Type = RestApiErrorType.Network,
                    Message = request.error,
                    Method = preparedConfig.Method,
                    Route = preparedConfig.Route,
                    Url = preparedConfig.Url,
                    NetworkResult = request.result,
                    ResponseBody = responseBody
                });
            }

            result.Method = preparedConfig.Method;
            result.Route = preparedConfig.Route;
            result.Url = preparedConfig.Url;
            result.HttpStatusCode = httpCode > 0 ? httpCode : null;
            result.RetryCount = preparedConfig.RetryCount;
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.ResponseBody = responseBody;

            #if UNITY_EDITOR
            Debugging.RestApiTraceBus.Record(preparedConfig, result, requestBodyForTrace);
            #endif

            operation.Complete(result);
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
                if (cfg.DownloadHandlerFactory == null)
                {
                    cfg.DownloadHandler = new DownloadHandlerBuffer();
                }
            }
            if (cfg.MaxRetries <= 0)
            {
                cfg.MaxRetries = 1;
            }
            if (cfg.MaxRedirects <= 0)
            {
                cfg.MaxRedirects = 5;
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
                DownloadHandlerFactory = config.DownloadHandlerFactory,
                UploadHandler = config.UploadHandler,
                TimeoutMs = config.TimeoutMs,
                RedirectLimit = config.RedirectLimit,
                FollowRedirect = config.FollowRedirect,
                MaxRedirects = config.MaxRedirects,
                RedirectHttpStatusCodes = config.RedirectHttpStatusCodes,
                NoAuthOnRedirect = config.NoAuthOnRedirect,
                StripHeadersOnRedirect = config.StripHeadersOnRedirect,
                AllowedHttpStatusCodes = config.AllowedHttpStatusCodes,
                MaxRetries = config.MaxRetries,
                RetryCount = config.RetryCount,
                DisableRetry = config.DisableRetry,
                NoAuth = config.NoAuth,
                AuthRetryAttempted = config.AuthRetryAttempted
            };

            cfg.Url = GetUrl(cfg.Route);

            if (cfg.MaxRetries <= 0)
            {
                cfg.MaxRetries = 1;
            }

            if (cfg.MaxRedirects <= 0)
            {
                cfg.MaxRedirects = 5;
            }

            if (cfg.FollowRedirect)
            {
                cfg.RedirectLimit = 0;
            }

            if (cfg.SerializedBody == null && cfg.Body != null && cfg.MultipartFormSections == null && cfg.UploadHandler == null)
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

            if (config.RedirectLimit.HasValue)
            {
                request.redirectLimit = config.RedirectLimit.Value;
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

        private static bool IsHttpSuccess(long httpCode, long[] allowedHttpStatusCodes)
        {
            if (httpCode >= 200 && httpCode <= 299)
            {
                return true;
            }

            if (allowedHttpStatusCodes == null || allowedHttpStatusCodes.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < allowedHttpStatusCodes.Length; i++)
            {
                if (allowedHttpStatusCodes[i] == httpCode)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRedirectStatus(long httpCode, long[] redirectHttpStatusCodes)
        {
            var codes = redirectHttpStatusCodes ?? DefaultRedirectHttpStatusCodes;
            if (codes == null || codes.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < codes.Length; i++)
            {
                if (codes[i] == httpCode)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ExtractRedirectLocation(UnityWebRequest request)
        {
            return request.GetResponseHeader("Location") ?? request.GetResponseHeader("location");
        }

        private static string ResolveRedirectUrl(string currentUrl, string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return null;
            }

            if (Uri.TryCreate(location, UriKind.Absolute, out var absolute))
            {
                return absolute.ToString();
            }

            if (Uri.TryCreate(currentUrl, UriKind.Absolute, out var baseUri) &&
                Uri.TryCreate(baseUri, location, out var resolved))
            {
                return resolved.ToString();
            }

            return location;
        }

        private static RestRequestConfig BuildRedirectConfig(RestRequestConfig config, string redirectUrl, long httpCode)
        {
            Dictionary<string, string> headers = null;
            if (config.StripHeadersOnRedirect == false && config.Headers != null)
            {
                headers = new Dictionary<string, string>(config.Headers);
                if (config.NoAuthOnRedirect)
                {
                    headers.Remove("Authorization");
                    headers.Remove("authorization");
                }
            }

            var method = config.Method;
            var body = config.Body;
            var serializedBody = config.SerializedBody;
            var multipartFormSections = config.MultipartFormSections;
            var uploadHandler = config.UploadHandler;

            if (httpCode == 303 && string.Equals(method, UnityWebRequest.kHttpVerbGET, StringComparison.OrdinalIgnoreCase) == false)
            {
                method = UnityWebRequest.kHttpVerbGET;
                body = null;
                serializedBody = null;
                multipartFormSections = null;
                uploadHandler = null;
            }

            var downloadHandler = config.DownloadHandler;
            if (config.DownloadHandlerFactory != null)
            {
                downloadHandler = null;
            }

            return new RestRequestConfig
            {
                Route = redirectUrl,
                Method = method,
                Body = body,
                SerializedBody = serializedBody,
                Headers = headers,
                MultipartFormSections = multipartFormSections,
                DownloadHandler = downloadHandler,
                DownloadHandlerFactory = config.DownloadHandlerFactory,
                UploadHandler = uploadHandler,
                TimeoutMs = config.TimeoutMs,
                RedirectLimit = 0,
                FollowRedirect = config.FollowRedirect,
                MaxRedirects = config.MaxRedirects,
                RedirectHttpStatusCodes = config.RedirectHttpStatusCodes,
                NoAuthOnRedirect = config.NoAuthOnRedirect,
                StripHeadersOnRedirect = config.StripHeadersOnRedirect,
                AllowedHttpStatusCodes = config.AllowedHttpStatusCodes,
                MaxRetries = config.MaxRetries,
                RetryCount = config.RetryCount,
                DisableRetry = config.DisableRetry,
                NoAuth = config.NoAuthOnRedirect ? true : config.NoAuth,
                AuthRetryAttempted = config.AuthRetryAttempted
            };
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

        #if UNITY_EDITOR
        private static string GetRequestBodyForTrace(RestRequestConfig preparedConfig)
        {
            if (preparedConfig == null)
            {
                return null;
            }

            if (preparedConfig.SerializedBody != null && preparedConfig.SerializedBody.Length > 0)
            {
                return Encoding.UTF8.GetString(preparedConfig.SerializedBody);
            }

            if (preparedConfig.MultipartFormSections != null)
            {
                return $"[multipart] sections={preparedConfig.MultipartFormSections.Count}";
            }

            if (preparedConfig.UploadHandler != null)
            {
                return $"[uploadHandler] {preparedConfig.UploadHandler.GetType().Name}";
            }

            return null;
        }
        #endif
    }
}
