using System.Collections;
using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Bootstrap;

namespace Plugins.MirraCloud.Example.Scripts.Core
{
    public class CloudInitializer : IBootstrapInitializable
    {
        private readonly IMirraCloudSdk _sdk;

        public CloudInitializer(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        public IEnumerator BootstrapInitialize()
        {
            _sdk.Initialize();
            yield return null;
        }
    }
}
