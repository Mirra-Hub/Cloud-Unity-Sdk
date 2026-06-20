using System;
using MirraCloud.Core.Chats;
using MirraCloud.Core.Chats.Dto;
using MirraCloud.Core.Chats.Events;
using MirraCloud.Core.Chats.Models;
using MirraCloud.Core.Realtime.Protocol;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// Wires all 10 ChatsService realtime events into a single log callback and returns an
    /// unsubscribe action. Isolated here so the Chat/realtime type usings stay out of the catalog.
    /// </summary>
    public static class ChatsEventLog
    {
        public static Action Subscribe(ChatsService chats, Action<string> log)
        {
            Action<RealtimeConnectionState> onState = s => log("connectionState → " + s);
            Action<string> onSub = id => log("subscribed → " + id);
            Action<ChatMessageDto> onMsg = m => log("messageReceived " + SandboxOps.SafeJson(m));
            Action<ChatMessageDto> onEdit = m => log("messageEdited " + SandboxOps.SafeJson(m));
            Action<RealtimeDeletePayload> onDel = p => log("messageDeleted " + SandboxOps.SafeJson(p));
            Action<ChatMemberEvent> onAdd = e => log("memberAdded " + SandboxOps.SafeJson(e));
            Action<ChatMemberEvent> onRem = e => log("memberRemoved " + SandboxOps.SafeJson(e));
            Action<ChatMemberBannedEvent> onBan = e => log("memberBanned " + SandboxOps.SafeJson(e));
            Action<string> onChDel = id => log("channelDeleted → " + id);
            Action<ChatErrorEvent> onErr = e => log("error " + SandboxOps.SafeJson(e));

            chats.OnConnectionStateChanged += onState;
            chats.OnSubscribedChannel += onSub;
            chats.OnMessageReceived += onMsg;
            chats.OnMessageEdited += onEdit;
            chats.OnMessageDeleted += onDel;
            chats.OnMemberAdded += onAdd;
            chats.OnMemberRemoved += onRem;
            chats.OnMemberBanned += onBan;
            chats.OnChannelDeleted += onChDel;
            chats.OnError += onErr;

            return () =>
            {
                chats.OnConnectionStateChanged -= onState;
                chats.OnSubscribedChannel -= onSub;
                chats.OnMessageReceived -= onMsg;
                chats.OnMessageEdited -= onEdit;
                chats.OnMessageDeleted -= onDel;
                chats.OnMemberAdded -= onAdd;
                chats.OnMemberRemoved -= onRem;
                chats.OnMemberBanned -= onBan;
                chats.OnChannelDeleted -= onChDel;
                chats.OnError -= onErr;
            };
        }
    }
}
