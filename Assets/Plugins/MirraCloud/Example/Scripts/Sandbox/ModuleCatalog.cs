using System.Collections.Generic;
using MirraCloud.Core;
using UnityEngine;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// Authors one <see cref="ModuleDescriptor"/> per SDK module. M0 ships two read-only
    /// modules end-to-end (no-arg queries); later milestones extend this catalog.
    /// </summary>
    public static class ModuleCatalog
    {
        public static List<ModuleDescriptor> Build(IMirraCloudSdk sdk)
        {
            return new List<ModuleDescriptor>
            {
                new ModuleDescriptor
                {
                    Id = "segments",
                    Title = "Segments",
                    Glyph = "≡",
                    Accent = Hex("#5AB6F0"),
                    Info = () => "Read-only. Returns the player's segments for the active project/branch.",
                    Controls =
                    {
                        new ControlDescriptor
                        {
                            Label = "Load Config",
                            Kind = ControlKind.Query,
                            Invoke = () => SandboxOps.Run(sdk.Segments.LoadConfigAsync())
                        }
                    }
                },
                new ModuleDescriptor
                {
                    Id = "remoteConfig",
                    Title = "Remote Config",
                    Glyph = "{ }",
                    Accent = Hex("#9AA0A6"),
                    Info = () => "Read-only. Fetches the active remote config for the project.",
                    Controls =
                    {
                        new ControlDescriptor
                        {
                            Label = "Load Config",
                            Kind = ControlKind.Query,
                            Invoke = () => SandboxOps.Run(sdk.RemoteConfig.LoadConfigAsync())
                        }
                    }
                },
            };
        }

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.gray;
        }
    }
}
