using System;
using MirraCloud.Core;
using MirraCloud.Core.Auth;
using MirraCloud.Core.Enums;
using Plugins.MirraCloud.Core.Services.PlayerAccount.Dto;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// PlayerAccount detail: a hero card (avatar + nickname/@handle + trait chips + lifetime stats +
    /// key/value meta + segment/AB tags), followed by the account's sub-profiles list.
    /// </summary>
    public sealed class PlayerAccountView : ServiceView
    {
        public PlayerAccountView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var heroSlot = AddSlot(16f);
            ViewBind.Load(Sdk.PlayerAccount.GetAccountAsync(), heroSlot, BuildHero);

            Content.Add(new SectionHeader("Profiles"));
            var profilesSlot = AddSlot();
            ViewBind.Load(
                Sdk.PlayerAccount.GetProfilesAsync(),
                profilesSlot,
                BuildProfiles,
                isEmpty: p => p == null || p.Length == 0,
                emptyView: () => EmptyState.Build("‍", "No sub-profiles for this account"));
        }

        private VisualElement BuildHero(PlayerAccountInfo a)
        {
            var card = new Card(Meta.Accent);

            var top = new VisualElement();
            top.AddToClassList("sc-hero__top");

            var avatar = new Avatar(72f);
            avatar.BindUrl(Images, a.AvatarUrl, a.Nickname);
            top.Add(avatar);

            var id = new VisualElement();
            id.AddToClassList("sc-hero__id");

            var name = new Label(string.IsNullOrEmpty(a.Nickname) ? "—" : a.Nickname);
            name.AddToClassList("sc-hero__name");
            id.Add(name);

            if (!string.IsNullOrEmpty(a.Username))
            {
                var handle = new Label("@" + a.Username);
                handle.AddToClassList("sc-hero__handle");
                id.Add(handle);
            }

            var traits = new VisualElement();
            traits.AddToClassList("sc-chip-row");
            traits.style.marginTop = 8;
            if (a.Gender != Gender.Unspecified)
            {
                traits.Add(new Chip(a.Gender.ToString(), ChipTone.Info));
            }
            if (a.Age > 0)
            {
                traits.Add(new Chip(a.Age + " yrs", ChipTone.Neutral));
            }
            traits.Add(new Chip(a.Country.ToString(), ChipTone.Neutral));
            traits.Add(new Chip(a.LanguageCode.ToString(), ChipTone.Neutral));
            if (!string.IsNullOrEmpty(a.Status))
            {
                traits.Add(new Chip(a.Status, ChipTone.Ok));
            }
            id.Add(traits);

            top.Add(id);
            card.Body.Add(top);

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginTop = 16;
            stats.Add(new StatTile("Sessions", "∑").Set(a.TotalSessions.ToString()));
            stats.Add(new StatTile("Active days", "▦").Set(a.TotalActiveDays.ToString()));
            stats.Add(new StatTile("Streak", "▲").Set(a.ConsecutiveActiveDays.ToString())
                .Highlight(a.ConsecutiveActiveDays > 0));
            stats.Add(new StatTile("Best streak", "★").Set(a.MaxConsecutiveActiveDays.ToString()));
            card.Body.Add(stats);

            var meta = new VisualElement();
            meta.AddToClassList("sc-kv-list");
            meta.Add(Kv("Member since", a.CreatedDate.ToLocalTime().ToString("yyyy-MM-dd")));
            meta.Add(Kv("Last login", a.LastLoginDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm")));
            meta.Add(Kv("Time zone", string.IsNullOrEmpty(a.TimeZone) ? "—" : a.TimeZone));
            meta.Add(Kv("Account ID", a.Id));
            meta.Add(Kv("Scope", a.ScopeId));
            card.Body.Add(meta);

            if (a.SegmentKeys != null && a.SegmentKeys.Length > 0)
            {
                card.Body.Add(Tags("Segments", a.SegmentKeys, ChipTone.Accent));
            }
            if (a.AbTestKeys != null && a.AbTestKeys.Length > 0)
            {
                card.Body.Add(Tags("A/B tests", a.AbTestKeys, ChipTone.Warn));
            }

            return card;
        }

        private VisualElement BuildProfiles(ProfileInfo[] profiles)
        {
            var list = new VisualElement();
            foreach (var p in profiles)
            {
                var row = new ListRow();

                var av = new Avatar(40f);
                av.BindUrl(Images, p.IconUrl, p.Nickname);
                row.SetLead(av);

                row.SetTitle(string.IsNullOrEmpty(p.Nickname) ? "—" : p.Nickname);
                row.SetSubtitle(string.IsNullOrEmpty(p.Username) ? p.Id : "@" + p.Username);

                if (!string.IsNullOrEmpty(p.Status))
                {
                    row.SetTrailing(new Chip(p.Status, ChipTone.Neutral));
                }

                list.Add(row);
            }
            return list;
        }

        private static VisualElement Kv(string key, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("sc-kv");

            var k = new Label(key);
            k.AddToClassList("sc-kv__k");
            var v = new Label(string.IsNullOrEmpty(value) ? "—" : value);
            v.AddToClassList("sc-kv__v");

            row.Add(k);
            row.Add(v);
            return row;
        }

        private static VisualElement Tags(string title, string[] tags, ChipTone tone)
        {
            var wrap = new VisualElement();
            wrap.style.marginTop = 14;
            wrap.Add(new SectionHeader(title, tags.Length.ToString()));

            var chips = new VisualElement();
            chips.AddToClassList("sc-chip-row");
            foreach (var t in tags)
            {
                chips.Add(new Chip(t, tone));
            }
            wrap.Add(chips);
            return wrap;
        }
    }
}
