using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace MirraCloud.Example.Showcase
{
    /// <summary>Runtime options for the showcase (set in the scene installer).</summary>
    public sealed class ShowcaseOptions
    {
        public bool DevForceServices;
    }

    /// <summary>
    /// Scene LifetimeScope for MC_Showcase. Auto-parents to the VContainer Root scope (which
    /// provides IMirraCloudSdk), grabs the scene's UIDocument, registers shared services, and
    /// runs <see cref="ShowcaseApp"/>.
    /// </summary>
    public sealed class ShowcaseInstaller : LifetimeScope
    {
        [Tooltip("Dev only: skip the auth gate and show the services screen (for visual QA without a backend).")]
        [SerializeField] private bool _devForceServices;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<UIDocument>();
            builder.Register<RemoteImageLoader>(Lifetime.Singleton);
            builder.RegisterInstance(new ShowcaseOptions { DevForceServices = _devForceServices });
            builder.RegisterEntryPoint<ShowcaseApp>();
        }
    }
}
