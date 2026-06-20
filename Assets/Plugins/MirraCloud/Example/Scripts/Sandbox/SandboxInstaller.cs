using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// Scene LifetimeScope for MC_Sandbox. Auto-parents to the VContainer Root scope
    /// (VContainerSettings.RootLifetimeScope), so IMirraCloudSdk + AuthService resolve from
    /// there. Grabs the scene's UIDocument and runs <see cref="SandboxApp"/> as the entry point.
    /// </summary>
    public sealed class SandboxInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<UIDocument>();
            builder.RegisterEntryPoint<SandboxApp>();
        }
    }
}
