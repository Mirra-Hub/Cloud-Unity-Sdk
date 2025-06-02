using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace MirraCloud
{
    public interface IBaseRestApiOperation : IEnumerator
    {
        public bool IsDone { get; }
        public bool IsError{ get; }
        public bool IsSuccess { get; }

        public string ErrorMessage { get; }
        public UnityWebRequest.Result Status { get; }
        
        public Task Task { get; }
        DownloadHandler DownloadHandler { get; }
    }
}