using System.Collections.Generic;
using MirraCloud.Core;
using UnityEngine;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// Authors one <see cref="ModuleDescriptor"/> per SDK module. M2 covers the safe,
    /// read-only modules (queries + a few string-parametrized reads). Mutating/realtime/
    /// monetization modules arrive in later milestones.
    /// </summary>
    public static class ModuleCatalog
    {
        public static List<ModuleDescriptor> Build(IMirraCloudSdk sdk)
        {
            return new List<ModuleDescriptor>
            {
                new ModuleDescriptor
                {
                    Id = "playerAccount", Title = "Player Account", Glyph = "@", Accent = Hex("#A78BFA"),
                    Info = () => "Read the authenticated account and its profiles.",
                    Controls =
                    {
                        Query("Get Account", _ => SandboxOps.Run(sdk.PlayerAccount.GetAccountAsync())),
                        Query("Get Profiles", _ => SandboxOps.Run(sdk.PlayerAccount.GetProfilesAsync())),
                        Param("Get Profile", v => SandboxOps.Run(sdk.PlayerAccount.GetProfileAsync(v[0])), F("profileId")),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "friends", Title = "Friends", Glyph = "+1", Accent = Hex("#EC5FA8"),
                    Info = () => "Read the friend list and incoming/outgoing requests.",
                    Controls =
                    {
                        Query("Get Friends", _ => SandboxOps.Run(sdk.Friends.GetFriendsAsync())),
                        Query("Get Requests", _ => SandboxOps.Run(sdk.Friends.GetRequestsAsync())),
                        Query("Get Incoming", _ => SandboxOps.Run(sdk.Friends.GetIncomingAsync())),
                        Query("Get Outgoing", _ => SandboxOps.Run(sdk.Friends.GetOutgoingAsync())),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "leaderboard", Title = "Leaderboard", Glyph = "▲", Accent = Hex("#F0606A"),
                    Info = () => "Initialize to discover leaderboard keys, then read configs/top entries. ID = business Key.",
                    Controls =
                    {
                        Query("Initialize (configs)", _ => SandboxOps.Run(sdk.Leaderboard.InitializeAsync())),
                        Param("Get Config", v => SandboxOps.Run(sdk.Leaderboard.GetConfigAsync(v[0])), F("leaderboardId (key)")),
                        Param("Get Top Entries", v => SandboxOps.Run(sdk.Leaderboard.GetLeaderboardTopEntries(v[0])), F("leaderboardId (key)")),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "segments", Title = "Segments", Glyph = "≡", Accent = Hex("#5AB6F0"),
                    Info = () => "Read the player's segments for the active project/branch.",
                    Controls =
                    {
                        Query("Load Config", _ => SandboxOps.Run(sdk.Segments.LoadConfigAsync())),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "remoteConfig", Title = "Remote Config", Glyph = "{ }", Accent = Hex("#B7A0E8"),
                    Info = () => "Fetch the active remote config for the project.",
                    Controls =
                    {
                        Query("Load Config", _ => SandboxOps.Run(sdk.RemoteConfig.LoadConfigAsync())),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "localization", Title = "Localization", Glyph = "Aa", Accent = Hex("#34D6A8"),
                    Info = () => "Read localized values by collection + key (server-seeded).",
                    Controls =
                    {
                        Param("Get All", v => SandboxOps.Run(sdk.Localization.GetAllLocalizationsAsync(v[0])), F("collectionId")),
                        Param("Get Values", v => SandboxOps.Run(sdk.Localization.GetValuesAsync(v[0], v[1])), F("collectionId"), F("key")),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "entities", Title = "Entities", Glyph = "[ ]", Accent = Hex("#54C7C7"),
                    Info = () => "Read the designer entity-config snapshot for the project/branch.",
                    Controls =
                    {
                        Query("Get Configs", _ => SandboxOps.Run(sdk.Entities.GetConfigsAsync())),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "deployment", Title = "Deployment", Glyph = "->", Accent = Hex("#9AA0A6"),
                    Info = () => "Resolve which branch a game version maps to.",
                    Controls =
                    {
                        Param("Resolve Branch", v => SandboxOps.Run(sdk.Deployment.ResolveBranchAsync(v[0])), F("version (e.g. 1.0.0)")),
                    }
                },
            };
        }

        private static ControlDescriptor Query(string label, System.Func<IReadOnlyList<string>, System.Threading.Tasks.Task<OpResult>> invoke)
        {
            return new ControlDescriptor { Label = label, Kind = ControlKind.Query, Invoke = invoke };
        }

        private static ControlDescriptor Param(string label, System.Func<IReadOnlyList<string>, System.Threading.Tasks.Task<OpResult>> invoke, params FieldDescriptor[] fields)
        {
            var c = new ControlDescriptor { Label = label, Kind = ControlKind.ParametrizedQuery, Invoke = invoke };
            c.Fields.AddRange(fields);
            return c;
        }

        private static FieldDescriptor F(string label, string def = null)
        {
            return new FieldDescriptor { Label = label, Default = def };
        }

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.gray;
        }
    }
}
