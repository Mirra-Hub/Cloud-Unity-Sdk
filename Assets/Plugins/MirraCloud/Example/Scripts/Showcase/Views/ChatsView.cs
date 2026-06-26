using System;
using MirraCloud.Core;
using MirraCloud.Core.Chats.Dto;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Chats detail: the SDK has no "list channels" REST endpoint, so the screen is lookup-driven —
    /// enter a channel ID (or a group ID to resolve its channel), then render the channel header +
    /// stats, member list, and recent message history. Realtime (WS) is intentionally out of scope.
    /// </summary>
    public sealed class ChatsView : ServiceView
    {
        private TextField _input;
        private VisualElement _detail;

        public ChatsView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var bar = new VisualElement();
            bar.AddToClassList("sc-chat-lookup");

            _input = new TextField { label = "Channel / Group ID" };
            _input.AddToClassList("sc-field");
            _input.AddToClassList("sc-chat-lookup__input");
            bar.Add(_input);

            var openBtn = new Button(() => OpenChannel(_input.value)) { text = "Open channel" };
            openBtn.AddToClassList("sc-btn");
            openBtn.AddToClassList("sc-btn--primary");
            bar.Add(openBtn);

            var groupBtn = new Button(() => LookupGroup(_input.value)) { text = "Lookup by group" };
            groupBtn.AddToClassList("sc-btn");
            bar.Add(groupBtn);

            Content.Add(bar);

            var hint = new Label("No list-channels API exists — paste a channel ID, or a group ID to resolve its channel.");
            hint.AddToClassList("sc-chat-hint");
            Content.Add(hint);

            _detail = AddSlot();
            Replace(_detail, EmptyState.Build("»", "No channel selected"));
        }

        private async void LookupGroup(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                Replace(_detail, EmptyState.Build("»", "Enter a group ID"));
                return;
            }
            Skeleton.Into(_detail);
            var op = Sdk.Chats.LookupGroupChannelAsync(groupId.Trim());
            await op.Task();
            var r = op.Result;
            if (r == null || !r.IsSuccess || r.Data == null)
            {
                Replace(_detail, ErrorState.Build(r?.Error));
                return;
            }
            OpenChannel(r.Data.ChannelId);
        }

        private void OpenChannel(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                Replace(_detail, EmptyState.Build("»", "Enter a channel ID"));
                return;
            }
            channelId = channelId.Trim();
            _detail.Clear();

            var headerSlot = new VisualElement();
            headerSlot.style.marginBottom = 14;
            _detail.Add(headerSlot);
            ViewBind.Load(Sdk.Chats.GetChannelAsync(channelId), headerSlot, BuildHeader);

            _detail.Add(new SectionHeader("Members"));
            var membersSlot = new VisualElement();
            membersSlot.style.marginBottom = 14;
            _detail.Add(membersSlot);
            ViewBind.Load(
                Sdk.Chats.GetMembersAsync(channelId),
                membersSlot,
                BuildMembers,
                isEmpty: m => m == null || m.Length == 0,
                emptyView: () => EmptyState.Build("@", "No members"));

            _detail.Add(new SectionHeader("Recent messages"));
            var msgsSlot = new VisualElement();
            _detail.Add(msgsSlot);
            ViewBind.Load(
                Sdk.Chats.GetMessagesAsync(channelId),
                msgsSlot,
                BuildMessages,
                isEmpty: m => m == null || m.Length == 0,
                emptyView: () => EmptyState.Build("»", "No messages yet"));
        }

        private VisualElement BuildHeader(ChatChannelDto ch)
        {
            var card = new Card(Meta.Accent);
            card.WithTitle(string.IsNullOrEmpty(ch.Name) ? ch.ChannelId : ch.Name, Meta.Accent);

            if (!string.IsNullOrEmpty(ch.Topic))
            {
                var topic = new Label(ch.Topic);
                topic.AddToClassList("sc-chat-topic");
                card.Body.Add(topic);
            }

            var chips = new VisualElement();
            chips.AddToClassList("sc-chip-row");
            if (!string.IsNullOrEmpty(ch.Type))
            {
                chips.Add(new Chip(ch.Type, ChipTone.Neutral));
            }
            if (!string.IsNullOrEmpty(ch.State))
            {
                chips.Add(new Chip(ch.State, ChipTone.Info));
            }
            if (ch.OwnerRef != null)
            {
                chips.Add(new Chip(ch.OwnerRef.Type + ":" + ch.OwnerRef.Id, ChipTone.Accent));
            }
            if (!string.IsNullOrEmpty(ch.TemplateKey))
            {
                chips.Add(new Chip("tpl: " + ch.TemplateKey, ChipTone.Neutral));
            }
            card.Body.Add(chips);

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginTop = 12;
            stats.Add(new StatTile("Messages", "#").Set(ch.LastMessageNumber.ToString()));
            stats.Add(new StatTile("Last msg", "~").Set(ch.LastMessageAt.HasValue
                ? ch.LastMessageAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                : "—"));
            stats.Add(new StatTile("Created", "+").Set(ch.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd")));
            stats.Add(new StatTile("Updated", "*").Set(ch.UpdatedAt.ToLocalTime().ToString("yyyy-MM-dd")));
            card.Body.Add(stats);

            return card;
        }

        private VisualElement BuildMembers(ChatMemberDto[] members)
        {
            var list = new VisualElement();
            foreach (var m in members)
            {
                var row = new ListRow();
                row.SetLead(new Avatar(32f).SetInitialsFor(m.ProfileId));
                row.SetTitle(m.ProfileId);
                row.SetSubtitle("joined " + m.JoinedAt.ToLocalTime().ToString("yyyy-MM-dd"));

                var trailing = new VisualElement();
                trailing.AddToClassList("sc-chip-row");
                trailing.Add(new Chip("read #" + m.LastReadMessageNumber, ChipTone.Neutral));
                row.SetTrailing(trailing);

                list.Add(row);
            }
            return list;
        }

        private VisualElement BuildMessages(ChatMessageDto[] msgs)
        {
            var list = new VisualElement();
            foreach (var m in msgs)
            {
                var row = new ListRow();
                row.SetLead(new Avatar(32f).SetInitialsFor(m.SenderId));
                row.SetTitle(m.SenderId);
                row.SetSubtitle(m.DeletedAt.HasValue
                    ? "(deleted)"
                    : (string.IsNullOrEmpty(m.Body) ? "—" : m.Body));

                var trailing = new VisualElement();
                trailing.AddToClassList("sc-chip-row");
                trailing.Add(new Chip("#" + m.Number, ChipTone.Neutral));
                if (m.EditedAt.HasValue)
                {
                    trailing.Add(new Chip("edited", ChipTone.Warn));
                }
                if (m.DeletedAt.HasValue)
                {
                    trailing.Add(new Chip("deleted", ChipTone.Bad));
                }
                if (m.TaggedMembers != null && m.TaggedMembers.Length > 0)
                {
                    trailing.Add(new Chip("@" + m.TaggedMembers.Length, ChipTone.Accent));
                }
                row.SetTrailing(trailing);

                list.Add(row);
            }
            return list;
        }
    }
}
