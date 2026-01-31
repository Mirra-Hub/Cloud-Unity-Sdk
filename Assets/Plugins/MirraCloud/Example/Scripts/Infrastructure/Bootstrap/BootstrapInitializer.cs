using System.Collections;
using System.Collections.Generic;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Bootstrap
{
    public class BootstrapInitializer
    {
        private readonly IEnumerable<IBootstrapInitializable> _initializables;

        public BootstrapInitializer(IEnumerable<IBootstrapInitializable> initializables)
        {
            _initializables = initializables;
        }

        public IEnumerator Initialize()
        {
            foreach (var initializable in _initializables)
            {
                yield return initializable.BootstrapInitialize();
            }
        }
    }
}