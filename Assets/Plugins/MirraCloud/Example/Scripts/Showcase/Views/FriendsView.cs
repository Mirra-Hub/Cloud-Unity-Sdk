using System;
using MirraCloud.Core;
using MirraCloud.Core.Auth;
using MirraCloud.Core.Enums;
using MirraCloud.Core.Friends.Dto;
using MirraCloud.Core.Friends.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Friends detail: a counts strip then three loaded sections — friends (avatar + presence dot +
    /// status chip), incoming requests, outgoing requests. The Friends SDK exposes no blocked/search
    /// read endpoint, so those are intentionally absent.
    /// </summary>
    public sealed class FriendsView : ServiceView
    {
        private StatTile _friendsTile;
        private StatTile _incomingTile;
        private StatTile _outgoingTile;

        public FriendsView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginBottom = 16;
            _friendsTile = new StatTile("Friends", "+1");
            _incomingTile = new StatTile("Incoming", "↓");
            _outgoingTile = new StatTile("Outgoing", "↑");
            stats.Add(_friendsTile);
            stats.Add(_incomingTile);
            stats.Add(_outgoingTile);
            Content.Add(stats);

            Content.Add(new SectionHeader("Friends"));
            var friendsSlot = AddSlot();
            ViewBind.Load(
                Sdk.Friends.GetFriendsAsync(),
                friendsSlot,
                data => { _friendsTile.Set(data.Length.ToString()); return RenderFriends(data); },
                isEmpty: d => d == null || d.Length == 0,
                emptyView: () => EmptyState.Build("+1", "No friends yet"));

            Content.Add(new SectionHeader("Incoming requests"));
            var incomingSlot = AddSlot();
            ViewBind.Load(
                Sdk.Friends.GetIncomingAsync(),
                incomingSlot,
                data => { _incomingTile.Set(data.Length.ToString()); return RenderRequests(data, true); },
                isEmpty: d => d == null || d.Length == 0,
                emptyView: () => EmptyState.Build("↓", "No incoming requests"));

            Content.Add(new SectionHeader("Outgoing requests"));
            var outgoingSlot = AddSlot();
            ViewBind.Load(
                Sdk.Friends.GetOutgoingAsync(),
                outgoingSlot,
                data => { _outgoingTile.Set(data.Length.ToString()); return RenderRequests(data, false); },
                isEmpty: d => d == null || d.Length == 0,
                emptyView: () => EmptyState.Build("↑", "No outgoing requests"));
        }

        private VisualElement RenderFriends(GetPlayerDto[] friends)
        {
            var list = new VisualElement();
            foreach (var p in friends)
            {
                var info = p.PlayerInfo;
                string name = info != null && !string.IsNullOrEmpty(info.Nickname) ? info.Nickname : p.PlayerId;

                var row = new ListRow();
                var av = new Avatar(40f);
                if (info != null && info.IconKey != null && info.IconKey.Source == KeySource.External)
                {
                    av.BindUrl(Images, info.IconKey.Key, name);
                }
                else
                {
                    av.SetInitialsFor(name);
                }
                av.SetPresence(PresenceColor(p.Status));
                row.SetLead(av);
                row.SetTitle(name);
                row.SetSubtitle(info != null
                    ? "Last seen " + info.LastLogin.ToLocalTime().ToString("yyyy-MM-dd")
                    : p.PlayerId);

                var trailing = new VisualElement();
                trailing.AddToClassList("sc-chip-row");
                trailing.Add(new Chip(p.Status.ToString(), PresenceTone(p.Status)));
                if (info != null && (info.Status == AccountStatus.Banned || info.Status == AccountStatus.Deleted))
                {
                    trailing.Add(new Chip(info.Status.ToString(), ChipTone.Bad));
                }
                row.SetTrailing(trailing);

                list.Add(row);
            }
            return list;
        }

        private VisualElement RenderRequests(GetFriendRequestDto[] rows, bool incoming)
        {
            var list = new VisualElement();
            foreach (var r in rows)
            {
                string otherId = incoming ? r.SourcePlayerId : r.TargetPlayerId;

                var row = new ListRow();
                row.SetLead(new Avatar(36f).SetInitialsFor(otherId));
                row.SetTitle(string.IsNullOrEmpty(otherId) ? "—" : otherId);
                row.SetSubtitle((incoming ? "Received " : "Sent ") + r.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
                row.SetTrailing(new Chip(r.Status.ToString(), RequestTone(r.Status)));
                list.Add(row);
            }
            return list;
        }

        private static Color? PresenceColor(ProfilePresenceStatus s)
        {
            switch (s)
            {
                case ProfilePresenceStatus.Online: return new Color(0.18f, 0.81f, 0.63f);
                case ProfilePresenceStatus.Away:
                case ProfilePresenceStatus.OnTheWay: return new Color(0.91f, 0.61f, 0.24f);
                case ProfilePresenceStatus.Busy: return new Color(0.94f, 0.50f, 0.54f);
                default: return null;
            }
        }

        private static ChipTone PresenceTone(ProfilePresenceStatus s)
        {
            switch (s)
            {
                case ProfilePresenceStatus.Online: return ChipTone.Ok;
                case ProfilePresenceStatus.Away:
                case ProfilePresenceStatus.OnTheWay: return ChipTone.Warn;
                case ProfilePresenceStatus.Busy: return ChipTone.Bad;
                default: return ChipTone.Neutral;
            }
        }

        private static ChipTone RequestTone(FriendRequestStatus s)
        {
            switch (s)
            {
                case FriendRequestStatus.Pending: return ChipTone.Info;
                case FriendRequestStatus.Accepted: return ChipTone.Ok;
                case FriendRequestStatus.Rejected: return ChipTone.Bad;
                default: return ChipTone.Neutral;
            }
        }
    }
}
