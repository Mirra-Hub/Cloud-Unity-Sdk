using System;
using System.Threading.Tasks;
using MirraCloud.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace MirraCloud
{
    public abstract class BaseRestApiOperation : CustomYieldInstruction, IBaseRestApiOperation, IDisposable
    {
        private readonly IJsonService _jsonService;
        public UnityWebRequest WebRequest { get; private set; }

        public bool IsDone { get; private set; }
        public bool IsError => WebRequest.result != UnityWebRequest.Result.Success;
        public bool IsSuccess => WebRequest.result == UnityWebRequest.Result.Success;

        public DownloadHandler DownloadHandler => WebRequest.downloadHandler;
        public string ErrorMessage => WebRequest.error;
        public UnityWebRequest.Result Status => WebRequest.result;
        
        public override bool keepWaiting => IsDone == false;
        public Task Task => TaskWait();

        protected BaseRestApiOperation(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }
        
        public void Initialize(UnityWebRequest request)
        {
            WebRequest = request;
        }
        
        public virtual void Complete()
        {
            IsDone = true;
        }

        public virtual void Dispose()
        {
        }

        public T GetData<T>()
        {
            if (IsSuccess == false || string.IsNullOrEmpty(DownloadHandler.text))
            {
                return default;
            }
            
            Debug.Log(DownloadHandler.text);
            
            return _jsonService.FromJson<T>(DownloadHandler.text);
        }

        private async Task TaskWait()
        {
            while (IsDone == false)
            {
                await Task.Yield();
            }
        }
    }
}