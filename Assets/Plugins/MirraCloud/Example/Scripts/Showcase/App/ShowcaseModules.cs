using UnityEngine;

namespace MirraCloud.Example.Showcase
{
    /// <summary>Display metadata for one SDK module card on the services screen.</summary>
    public struct ServiceMeta
    {
        public string Id;
        public string Title;
        public string Glyph;
        public Color Accent;
    }

    /// <summary>The full set of SDK modules shown as cards. Each opens its IServiceView
    /// (added per-milestone); until then it falls through to a "coming soon" detail.</summary>
    public static class ShowcaseModules
    {
        public static readonly ServiceMeta[] All = Build();

        private static ServiceMeta[] Build()
        {
            return new[]
            {
                M("playerAccount", "Player Account", "@", "#A78BFA"),
                M("friends", "Friends", "+1", "#EC5FA8"),
                M("groups", "Groups", "##", "#9B8CFF"),
                M("chats", "Chats", "»", "#E0479E"),
                M("leaderboard", "Leaderboard", "▲", "#F0606A"),
                M("tournaments", "Tournaments", "TR", "#E89B3D"),
                M("challenges", "Challenges", "CH", "#B6D94C"),
                M("dailyRewards", "Daily Rewards", "DR", "#F2843B"),
                M("economy", "Economy", "$", "#5BD15B"),
                M("purchases", "Purchases", "$$", "#F5A623"),
                M("promoCodes", "Promo Codes", "%", "#FF7AA8"),
                M("cloudSave", "Cloud Save", "DB", "#6BD0E0"),
                M("entities", "Entities", "[ ]", "#54C7C7"),
                M("remoteConfig", "Remote Config", "{ }", "#B7A0E8"),
                M("localization", "Localization", "Aa", "#34D6A8"),
                M("segments", "Segments", "≡", "#5AB6F0"),
                M("assets", "Assets Storage", "FS", "#6FC0F0"),
                M("analytics", "Analytics", "≈", "#7FB0C0"),
                M("profanity", "Profanity Filter", "*", "#E86A6A"),
                M("cloudCode", "Cloud Code", "</>", "#7FD97F"),
                M("deployment", "Deployment", "->", "#9AA0A6"),
                M("webview", "WebView", "WV", "#46C0B0"),
            };
        }

        private static ServiceMeta M(string id, string title, string glyph, string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return new ServiceMeta { Id = id, Title = title, Glyph = glyph, Accent = c };
        }
    }
}
