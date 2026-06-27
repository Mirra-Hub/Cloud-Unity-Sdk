using System;
using MirraCloud.Core;
using MirraCloud.Core.Groups.Dto.Response;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Groups detail (master/detail): the player's groups list (avatar + name + member-count) on top;
    /// selecting a group loads its full card (visibility/join-policy/chat/tags + stats) and a members
    /// table with the owner highlighted. Read-only — no create/join/moderation affordances.
    /// </summary>
    public sealed class GroupsView : ServiceView
    {
        private VisualElement _detail;

        public GroupsView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            Content.Add(new SectionHeader("My groups"));
            var listSlot = AddSlot();
            ViewBind.Load(
                Sdk.Groups.GetMyGroupsAsync(),
                listSlot,
                RenderList,
                isEmpty: r => r == null || r.Items == null || r.Items.Length == 0,
                emptyView: () => EmptyState.Build("##", "You are not in any groups yet"));

            _detail = AddSlot();
            Replace(_detail, EmptyState.Build("##", "Select a group to see details"));
        }

        private VisualElement RenderList(PaginatedResult<GroupListItemDto> page)
        {
            var list = new VisualElement();
            foreach (var g in page.Items)
            {
                var captured = g;
                var row = new ListRow();
                var av = new Avatar(40f);
                av.BindUrl(Images, g.Avatar, g.Name);
                row.SetLead(av);
                row.SetTitle(string.IsNullOrEmpty(g.Name) ? "—" : g.Name);
                row.SetSubtitle(string.IsNullOrEmpty(g.Description) ? g.Visibility : g.Description);
                row.SetTrailing(new Chip(g.MemberCount + "/" + g.MaxMembers, ChipTone.Neutral));
                row.RegisterCallback<ClickEvent>(_ => OpenGroup(captured.GroupId));
                list.Add(row);
            }
            return list;
        }

        private void OpenGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                return;
            }
            _detail.Clear();

            var cardSlot = new VisualElement();
            cardSlot.style.marginBottom = 14;
            _detail.Add(cardSlot);
            ViewBind.Load(Sdk.Groups.GetAsync(groupId), cardSlot, BuildGroupCard);

            _detail.Add(new SectionHeader("Members"));
            var membersSlot = new VisualElement();
            _detail.Add(membersSlot);
            ViewBind.Load(
                Sdk.Groups.GetMembersAsync(groupId),
                membersSlot,
                BuildMembers,
                isEmpty: r => r == null || r.Items == null || r.Items.Length == 0,
                emptyView: () => EmptyState.Build("@", "No members"));
        }

        private VisualElement BuildGroupCard(GroupDto g)
        {
            var card = new Card(Meta.Accent);
            card.WithTitle(string.IsNullOrEmpty(g.Name) ? g.GroupId : g.Name, Meta.Accent);

            var top = new VisualElement();
            top.style.flexDirection = FlexDirection.Row;
            top.style.alignItems = Align.Center;
            var av = new Avatar(56f);
            av.BindUrl(Images, g.Avatar, g.Name);
            av.style.marginRight = 14;
            top.Add(av);

            var chips = new VisualElement();
            chips.AddToClassList("sc-chip-row");
            chips.style.flexGrow = 1;
            chips.Add(new Chip(g.Visibility, VisibilityTone(g.Visibility)));
            chips.Add(new Chip(g.JoinPolicy, ChipTone.Info));
            if (g.ChatConfig != null)
            {
                chips.Add(new Chip(g.ChatConfig.ChatEnabled ? "Chat on" : "Chat off",
                    g.ChatConfig.ChatEnabled ? ChipTone.Ok : ChipTone.Neutral));
            }
            if (g.Tag != null)
            {
                foreach (var t in g.Tag)
                {
                    chips.Add(new Chip(t, ChipTone.Neutral));
                }
            }
            top.Add(chips);
            card.Body.Add(top);

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginTop = 12;
            stats.Add(new StatTile("Members", "+1").Set(g.MemberCount + "/" + g.MaxMembers).Highlight(g.MemberCount >= g.MaxMembers));
            stats.Add(new StatTile("Created", "+").Set(g.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd")));
            stats.Add(new StatTile("Updated", "*").Set(g.UpdatedAt.ToLocalTime().ToString("yyyy-MM-dd")));
            card.Body.Add(stats);

            if (!string.IsNullOrEmpty(g.Description))
            {
                var desc = new Label(g.Description);
                desc.AddToClassList("sc-chat-topic");
                desc.style.marginTop = 10;
                card.Body.Add(desc);
            }

            return card;
        }

        private VisualElement BuildMembers(PaginatedResult<MemberDto> page)
        {
            var cols = new[]
            {
                new DataColumn { Header = "", FixedWidth = true, Px = 44, Align = "center", Cell = o => new Avatar(32f).SetInitialsFor(((MemberDto)o).ProfileId) },
                new DataColumn { Header = "PLAYER", Grow = 2f, Cell = o => new Label(((MemberDto)o).ProfileId) },
                new DataColumn { Header = "ROLE", Grow = 1f, Cell = o => new Chip(string.IsNullOrEmpty(((MemberDto)o).RoleName) ? "—" : ((MemberDto)o).RoleName, ((MemberDto)o).IsOwner ? ChipTone.Accent : ChipTone.Neutral) },
                new DataColumn { Header = "JOINED", FixedWidth = true, Px = 110, Align = "right", Cell = o => new Label(((MemberDto)o).JoinedAt.ToLocalTime().ToString("yyyy-MM-dd")) },
            };
            return new DataTable(cols).Bind(page.Items, row => ((MemberDto)row).IsOwner);
        }

        private static ChipTone VisibilityTone(string visibility)
        {
            if (visibility == "Public") return ChipTone.Ok;
            if (visibility == "Hidden") return ChipTone.Bad;
            return ChipTone.Warn;
        }
    }
}
