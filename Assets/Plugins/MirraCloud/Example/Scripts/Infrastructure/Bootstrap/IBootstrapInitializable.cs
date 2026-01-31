using System.Collections;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Bootstrap
{
    public interface IBootstrapInitializable
    {
        public IEnumerator BootstrapInitialize();
    }
}