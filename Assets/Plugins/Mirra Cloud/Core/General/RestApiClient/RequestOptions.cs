using UnityEngine.Networking;

namespace MirraCloud.Core
{
    public class RequestOptions
    {
        public DownloadHandler DownloadHandler;
        public object Body;
        public string Method;

        public RequestOptions()
        {
        }
    }
}