using JetBrains.Annotations;

namespace MirraCloud
{
    public struct UpdatePlayerInfoOptions
    {
        [CanBeNull] public string Name;
        public uint? Age;
    }
}