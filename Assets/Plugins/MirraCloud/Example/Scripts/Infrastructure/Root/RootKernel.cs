using System.Collections.Generic;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.Root
{
    public class RootKernel : MonoBehaviour
    {
        private IEnumerable<IRootInitializable> _initializables;
        private IEnumerable<IRootDisposable> _disposables;

        [InjectDep]
        public void Construct(IEnumerable<IRootInitializable> initializables, IEnumerable<IRootDisposable> disposables)
        {
            _disposables = disposables;
            _initializables = initializables;
        }

        private void Awake()
        {
            foreach (var initializable in _initializables)
            {
                initializable.RootInitialize();
            }
        }

        private void OnDestroy()
        {
            foreach (var disposable in _disposables)
            {
                disposable.RootDispose();
            }
        }
    }
}