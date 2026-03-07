using System;

namespace MirraCloud.Core.CloudSave
{
    [Flags]
    public enum PrincipalMask
    {
        None = 0,
        Owner = 1,
        Other = 2,
        Server = 4
    }
}
