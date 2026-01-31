using System;
using VContainer;

namespace MirraCloud.Example.Infrastructure.DI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class InjectDepAttribute : InjectAttribute
    {
    }
}
