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
                new ModuleDescriptor
                {
                    Id = "economy", Title = "Economy", Glyph = "$", Accent = Hex("#5BD15B"),
                    Info = () => "Currencies/items/energies. LoadConfigs first. Mutating ops write real balances.",
                    Controls =
                    {
                        Query("Load Configs", _ => SandboxOps.Run(sdk.Economy.LoadConfigsAsync())),
                        Query("Load Inventory", _ => SandboxOps.Run(sdk.Economy.LoadInventoryAsync())),
                        Query("Get Energies", _ => SandboxOps.Run(sdk.Economy.GetEnergiesAsync())),
                        Param("Get Energy", v => SandboxOps.Run(sdk.Economy.GetEnergyAsync(v[0])), F("energyId")),
                        Mutate("Add Item", v => SandboxOps.Run(sdk.Economy.AddItemAsync(v[0], SandboxParse.Int(v[1]))), F("itemId"), Fi("amount", "1")),
                        Mutate("Subtract Item", v => SandboxOps.Run(sdk.Economy.SubtractItemAsync(v[0], SandboxParse.Int(v[1]))), F("itemId"), Fi("amount", "1")),
                        Mutate("Consume Item", v => SandboxOps.Run(sdk.Economy.ConsumeItemAsync(v[0])), F("itemId")),
                        Mutate("Spend Energy", v => SandboxOps.Run(sdk.Economy.SpendEnergyAsync(v[0], SandboxParse.Int(v[1]))), F("energyId"), Fi("amount", "1")),
                        Mutate("Add Energy", v => SandboxOps.Run(sdk.Economy.AddEnergyAsync(v[0], SandboxParse.Int(v[1]))), F("energyId"), Fi("amount", "1")),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "cloudSave", Title = "Cloud Save", Glyph = "DB", Accent = Hex("#6BD0E0"),
                    Info = () => "Read player/global data + delete by key. (Upsert/query DTO-builders land later.)",
                    Controls =
                    {
                        Query("Get Player Data", _ => SandboxOps.Run(sdk.CloudSave.GetPlayerDataAsync())),
                        Query("Load Global Data", _ => SandboxOps.Run(sdk.CloudSave.LoadGlobalDataAsync())),
                        Mutate("Delete Player Data", v => SandboxOps.Run(sdk.CloudSave.DeletePlayerDataAsync(v[0])), F("key")),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "challenges", Title = "Challenges", Glyph = "CH", Accent = Hex("#B6D94C"),
                    Info = () => "Initialize for keys. challengeId = business Key. Join/Submit/Claim mutate player state.",
                    Controls =
                    {
                        Query("Initialize", _ => SandboxOps.Run(sdk.Challenges.InitializeAsync())),
                        Param("Get Config", v => SandboxOps.Run(sdk.Challenges.GetConfigAsync(v[0])), F("challengeId (key)")),
                        Param("Get Top", v => SandboxOps.Run(sdk.Challenges.GetTopAsync(v[0])), F("challengeId (key)")),
                        Param("Get My Top", v => SandboxOps.Run(sdk.Challenges.GetMyTopAsync(v[0])), F("challengeId (key)")),
                        Mutate("Join", v => SandboxOps.Run(sdk.Challenges.JoinAsync(v[0])), F("challengeId (key)")),
                        Mutate("Leave", v => SandboxOps.Run(sdk.Challenges.LeaveAsync(v[0])), F("challengeId (key)")),
                        Mutate("Submit Score", v => SandboxOps.Run(sdk.Challenges.SubmitScoreAsync(v[0], SandboxParse.Double(v[1]))), F("challengeId (key)"), Ff("score", "100")),
                        Mutate("Claim Reward", v => SandboxOps.Run(sdk.Challenges.ClaimRewardAsync(v[0])), F("challengeId (key)")),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "tournaments", Title = "Tournaments", Glyph = "TR", Accent = Hex("#E89B3D"),
                    Info = () => "Initialize for keys. tournamentId = Key, tableId from config. GetRewards(reset=true) consumes!",
                    Controls =
                    {
                        Query("Initialize", _ => SandboxOps.Run(sdk.Tournaments.InitializeAsync())),
                        Param("Get Config", v => SandboxOps.Run(sdk.Tournaments.GetConfigAsync(v[0])), F("tournamentId (key)")),
                        Param("Get Top", v => SandboxOps.Run(sdk.Tournaments.GetTopAsync(v[0], v[1])), F("tournamentId (key)"), F("tableId")),
                        Param("Get League Meta", v => SandboxOps.Run(sdk.Tournaments.GetPlayerLeagueMetaAsync(v[0])), F("tournamentId (key)")),
                        Param("Get Rewards", v => SandboxOps.Run(sdk.Tournaments.GetRewardsAsync(SandboxParse.Bool(v[0]))), Fb("reset (consumes!)", "false")),
                        Mutate("Submit Score", v => SandboxOps.Run(sdk.Tournaments.SubmitScoreAsync(v[0], SandboxParse.Double(v[1]))), F("tournamentId (key)"), Ff("score", "100")),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "dailyRewards", Title = "Daily Rewards", Glyph = "DR", Accent = Hex("#F2843B"),
                    Info = () => "Calendars + status; Claim grants rewards (once per UTC reset).",
                    Controls =
                    {
                        Query("Get Calendars", _ => SandboxOps.Run(sdk.DailyRewards.GetCalendarsAsync())),
                        Query("Get Status (all)", _ => SandboxOps.Run(sdk.DailyRewards.GetStatusAsync())),
                        Param("Get Status (calendar)", v => SandboxOps.Run(sdk.DailyRewards.GetStatusAsync(v[0])), F("calendarId")),
                        Mutate("Claim", v => SandboxOps.Run(sdk.DailyRewards.ClaimAsync(v[0])), F("calendarId")),
                    }
                },
                new ModuleDescriptor
                {
                    Id = "chats", Title = "Chats", Glyph = "»", Accent = Hex("#E0479E"),
                    Info = () => "REST (create/get/members/messages/join/leave) works without realtime. Realtime (connect/subscribe/send/edit/delete/markRead) needs ConnectAsync first, else 'not_connected'. Events stream into the log below.",
                    HasEventLog = true,
                    Subscribe = log => ChatsEventLog.Subscribe(sdk.Chats, log),
                    Controls =
                    {
                        Query("Connect (realtime)", _ => SandboxOps.RunRt(sdk.Chats.ConnectAsync())),
                        Query("Disconnect", _ => SandboxOps.RunRt(sdk.Chats.DisconnectAsync())),
                        Param("Create Channel", v => SandboxOps.Run(sdk.Chats.CreateChannelAsync(v[0], v[1])), F("name"), F("templateKey")),
                        Param("Get Channel", v => SandboxOps.Run(sdk.Chats.GetChannelAsync(v[0])), F("channelId")),
                        Param("Get Members", v => SandboxOps.Run(sdk.Chats.GetMembersAsync(v[0])), F("channelId")),
                        Param("Get Messages", v => SandboxOps.Run(sdk.Chats.GetMessagesAsync(v[0])), F("channelId")),
                        Param("Lookup Group Channel", v => SandboxOps.Run(sdk.Chats.LookupGroupChannelAsync(v[0])), F("groupId")),
                        Param("Subscribe", v => SandboxOps.RunRt(sdk.Chats.SubscribeAsync(v[0])), F("channelId")),
                        Param("Unsubscribe", v => SandboxOps.RunRt(sdk.Chats.UnsubscribeAsync(v[0])), F("channelId")),
                        Param("Send Message", v => SandboxOps.RunRt(sdk.Chats.SendMessageAsync(v[0], v[1])), F("channelId"), F("body")),
                        Param("Edit Message", v => SandboxOps.RunRt(sdk.Chats.EditMessageAsync(v[0], v[1], v[2])), F("channelId"), F("messageId"), F("body")),
                        Param("Mark As Read", v => SandboxOps.RunRt(sdk.Chats.MarkAsReadAsync(v[0], (long)SandboxParse.Int(v[1]))), F("channelId"), Fi("messageNumber")),
                        Param("Join", v => SandboxOps.Run(sdk.Chats.JoinAsync(v[0])), F("channelId")),
                        Mutate("Leave", v => SandboxOps.Run(sdk.Chats.LeaveAsync(v[0])), F("channelId")),
                        Mutate("Delete Message", v => SandboxOps.RunRt(sdk.Chats.DeleteMessageAsync(v[0], v[1])), F("channelId"), F("messageId")),
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

        // Mutating / state-changing call: styled red (no confirm — this is a raw dev tool).
        private static ControlDescriptor Mutate(string label, System.Func<IReadOnlyList<string>, System.Threading.Tasks.Task<OpResult>> invoke, params FieldDescriptor[] fields)
        {
            var c = new ControlDescriptor { Label = label, Kind = ControlKind.Action, Destructive = true, Invoke = invoke };
            c.Fields.AddRange(fields);
            return c;
        }

        private static FieldDescriptor F(string label, string def = null)
        {
            return new FieldDescriptor { Label = label, Default = def };
        }

        private static FieldDescriptor Fi(string label, string def = null)
        {
            return new FieldDescriptor { Label = label, Default = def, Type = FieldType.Int };
        }

        private static FieldDescriptor Ff(string label, string def = null)
        {
            return new FieldDescriptor { Label = label, Default = def, Type = FieldType.Float };
        }

        private static FieldDescriptor Fb(string label, string def = null)
        {
            return new FieldDescriptor { Label = label, Default = def, Type = FieldType.Bool };
        }

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.gray;
        }
    }
}
