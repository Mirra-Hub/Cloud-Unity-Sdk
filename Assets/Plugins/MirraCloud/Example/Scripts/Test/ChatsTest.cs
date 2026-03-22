using MirraCloud.Core;
using MirraCloud.Core.Chats.Dto;
using MirraCloud.Core.Chats.Events;
using MirraCloud.Core.Chats.Models;
using MirraCloud.Core.Realtime.Protocol;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class ChatsTest : MonoBehaviour
    {
        [Header("Channel (create via Admin API first)")]
        [SerializeField] private string _channelId;

        [Header("Messages")]
        [SerializeField] private string _messageBody = "Hello from SDK!";
        [SerializeField] private string _editedBody = "Edited message";

        private string _lastSentMessageId;
        private bool _connected;

        private void OnEnable()
        {
            MirraCloudSDK.Chats.OnConnectionStateChanged += HandleConnectionStateChanged;
            MirraCloudSDK.Chats.OnSubscribed += HandleSubscribed;
            MirraCloudSDK.Chats.OnMessageReceived += HandleMessageReceived;
            MirraCloudSDK.Chats.OnMessageEdited += HandleMessageEdited;
            MirraCloudSDK.Chats.OnMessageDeleted += HandleMessageDeleted;
            MirraCloudSDK.Chats.OnMemberAdded += HandleMemberAdded;
            MirraCloudSDK.Chats.OnMemberRemoved += HandleMemberRemoved;
            MirraCloudSDK.Chats.OnChannelDeleted += HandleChannelDeleted;
            MirraCloudSDK.Chats.OnError += HandleError;
        }

        private void OnDisable()
        {
            MirraCloudSDK.Chats.OnConnectionStateChanged -= HandleConnectionStateChanged;
            MirraCloudSDK.Chats.OnSubscribed -= HandleSubscribed;
            MirraCloudSDK.Chats.OnMessageReceived -= HandleMessageReceived;
            MirraCloudSDK.Chats.OnMessageEdited -= HandleMessageEdited;
            MirraCloudSDK.Chats.OnMessageDeleted -= HandleMessageDeleted;
            MirraCloudSDK.Chats.OnMemberAdded -= HandleMemberAdded;
            MirraCloudSDK.Chats.OnMemberRemoved -= HandleMemberRemoved;
            MirraCloudSDK.Chats.OnChannelDeleted -= HandleChannelDeleted;
            MirraCloudSDK.Chats.OnError -= HandleError;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Connect();
            if (Input.GetKeyDown(KeyCode.Alpha2)) JoinChannel();
            if (Input.GetKeyDown(KeyCode.Alpha3)) Subscribe();
            if (Input.GetKeyDown(KeyCode.Alpha4)) SendMessage();
            if (Input.GetKeyDown(KeyCode.Alpha5)) EditLastMessage();
            if (Input.GetKeyDown(KeyCode.Alpha6)) DeleteLastMessage();
            if (Input.GetKeyDown(KeyCode.Alpha7)) GetHistory();
            if (Input.GetKeyDown(KeyCode.Alpha8)) GetMembers();
            if (Input.GetKeyDown(KeyCode.Alpha9)) GetChannel();
            if (Input.GetKeyDown(KeyCode.Alpha0)) LeaveChannel();
            if (Input.GetKeyDown(KeyCode.Minus)) Unsubscribe();
            if (Input.GetKeyDown(KeyCode.Equals)) Disconnect();
        }
        
        private void Connect()
        {
            Debug.Log("[ChatsTest] Connecting to realtime...");
            var op = MirraCloudSDK.Chats.ConnectAsync();
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] Connect failed: {completed.Result.Error?.Message}");
                    return;
                }
                Debug.Log("[ChatsTest] Connected!");
            };
        }

        private void Disconnect()
        {
            Debug.Log("[ChatsTest] Disconnecting...");
            var op = MirraCloudSDK.Chats.DisconnectAsync();
            op.OnCompleted += completed =>
            {
                Debug.Log("[ChatsTest] Disconnected.");
            };
        }

        private void JoinChannel()
        {
            Debug.Log($"[ChatsTest] Joining channel {_channelId}...");
            var op = MirraCloudSDK.Chats.JoinAsync(_channelId);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] Join failed: {completed.Result.Error?.Message}");
                    return;
                }
                Debug.Log("[ChatsTest] Joined channel.");
            };
        }

        private void LeaveChannel()
        {
            Debug.Log($"[ChatsTest] Leaving channel {_channelId}...");
            var op = MirraCloudSDK.Chats.LeaveAsync(_channelId);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] Leave failed: {completed.Result.Error?.Message}");
                    return;
                }
                Debug.Log("[ChatsTest] Left channel.");
            };
        }

        private void Subscribe()
        {
            Debug.Log($"[ChatsTest] Subscribing to channel {_channelId}...");
            var op = MirraCloudSDK.Chats.SubscribeAsync(_channelId);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] Subscribe failed: {completed.Result.Error?.Message}");
                    return;
                }
                Debug.Log("[ChatsTest] Subscribed.");
            };
        }

        private void Unsubscribe()
        {
            Debug.Log($"[ChatsTest] Unsubscribing from channel {_channelId}...");
            var op = MirraCloudSDK.Chats.UnsubscribeAsync(_channelId);
            op.OnCompleted += completed =>
            {
                Debug.Log("[ChatsTest] Unsubscribed.");
            };
        }

        // TODO: При отправке событий в канал добавить хранение подключений к каналам (продублировать логику из бэкенда). Добавлять в хэшсет при subscribe
        private void SendMessage()
        {
            Debug.Log($"[ChatsTest] Sending message: {_messageBody}");
            var op = MirraCloudSDK.Chats.SendMessageAsync(_channelId, _messageBody);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] Send failed: {completed.Result.Error?.Message}");
                    return;
                }
                var msg = completed.Result.Data;
                _lastSentMessageId = msg.MessageId;
                Debug.Log($"[ChatsTest] Sent message #{msg.Number} id={msg.MessageId}");
            };
        }

        private void EditLastMessage()
        {
            if (string.IsNullOrEmpty(_lastSentMessageId))
            {
                Debug.LogWarning("[ChatsTest] No message to edit. Send one first (key 4).");
                return;
            }

            Debug.Log($"[ChatsTest] Editing message {_lastSentMessageId}...");
            var op = MirraCloudSDK.Chats.EditMessageAsync(_channelId, _lastSentMessageId, _editedBody);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] Edit failed: {completed.Result.Error?.Message}");
                    return;
                }
                Debug.Log($"[ChatsTest] Message edited. New body: {completed.Result.Data.Body}");
            };
        }

        private void DeleteLastMessage()
        {
            if (string.IsNullOrEmpty(_lastSentMessageId))
            {
                Debug.LogWarning("[ChatsTest] No message to delete. Send one first (key 4).");
                return;
            }

            Debug.Log($"[ChatsTest] Deleting message {_lastSentMessageId}...");
            var op = MirraCloudSDK.Chats.DeleteMessageAsync(_channelId, _lastSentMessageId);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] Delete failed: {completed.Result.Error?.Message}");
                    return;
                }
                Debug.Log("[ChatsTest] Message deleted.");
                _lastSentMessageId = null;
            };
        }

        private void GetHistory()
        {
            Debug.Log($"[ChatsTest] Loading message history for channel {_channelId}...");
            var op = MirraCloudSDK.Chats.GetMessagesAsync(_channelId, limit: 20);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] GetMessages failed: {completed.Result.Error?.Message}");
                    return;
                }
                var messages = completed.Result.Data;
                Debug.Log($"[ChatsTest] Got {messages.Length} messages:");
                foreach (var m in messages)
                {
                    Debug.Log($"  #{m.Number} [{m.SenderId}]: {m.Body}");
                }
            };
        }

        private void GetMembers()
        {
            Debug.Log($"[ChatsTest] Loading members for channel {_channelId}...");
            var op = MirraCloudSDK.Chats.GetMembersAsync(_channelId);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] GetMembers failed: {completed.Result.Error?.Message}");
                    return;
                }
                var members = completed.Result.Data;
                Debug.Log($"[ChatsTest] {members.Length} members:");
                foreach (var m in members)
                {
                    Debug.Log($"  {m.ProfileId} joined={m.JoinedAt}");
                }
            };
        }

        private void GetChannel()
        {
            Debug.Log($"[ChatsTest] Getting channel {_channelId}...");
            var op = MirraCloudSDK.Chats.GetChannelAsync(_channelId);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[ChatsTest] GetChannel failed: {completed.Result.Error?.Message}");
                    return;
                }
                var ch = completed.Result.Data;
                Debug.Log($"[ChatsTest] Channel: {ch.Name} type={ch.Type} state={ch.State} lastMsg#{ch.LastMessageNumber}");
            };
        }

        private void HandleConnectionStateChanged(RealtimeConnectionState state)
        {
            _connected = state == RealtimeConnectionState.Connected;
            Debug.Log($"[ChatsTest] Connection state: {state}");
        }

        private void HandleSubscribed(string channelId)
        {
            Debug.Log($"[ChatsTest] Subscribed to channel: {channelId}");
        }

        private void HandleMessageReceived(ChatMessageDto msg)
        {
            Debug.Log($"[ChatsTest] Message received: #{msg.Number} [{msg.SenderId}]: {msg.Body}");
        }

        private void HandleMessageEdited(ChatMessageDto msg)
        {
            Debug.Log($"[ChatsTest] Message edited: #{msg.Number} new body: {msg.Body}");
        }

        private void HandleMessageDeleted(RealtimeDeletePayload payload)
        {
            Debug.Log($"[ChatsTest] Message deleted: {payload.MessageId} in channel {payload.ChannelId}");
        }

        private void HandleMemberAdded(ChatMemberEvent evt)
        {
            Debug.Log($"[ChatsTest] Member added: {evt.ProfileId} to channel {evt.ChannelId}");
        }

        private void HandleMemberRemoved(ChatMemberEvent evt)
        {
            Debug.Log($"[ChatsTest] Member removed: {evt.ProfileId} from channel {evt.ChannelId}");
        }

        private void HandleChannelDeleted(string channelId)
        {
            Debug.Log($"[ChatsTest] Channel deleted: {channelId}");
        }

        private void HandleError(ChatErrorEvent err)
        {
            Debug.LogError($"[ChatsTest] Error [{err.Code}]: {err.Message}");
        }
    }
}
