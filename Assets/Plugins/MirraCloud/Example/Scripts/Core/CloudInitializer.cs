using System.Collections;
using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Bootstrap;

namespace Plugins.MirraCloud.Example.Scripts.Core
{
    public class CloudInitializer : IBootstrapInitializable
    {
        public IEnumerator BootstrapInitialize()
        {
            MirraCloudSDK.Initialize();
            yield return null;
        }
    }
}