using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MirraCloud.Core.Chats.Dto;
using MirraCloud.Core.Chats.Events;
using MirraCloud.Core.Chats.Models;
using MirraCloud.Core.Logger;
using MirraCloud.Core.Realtime.Abstractions;
using MirraCloud.Core.Realtime.Auth;
using MirraCloud.Core.Realtime.Connection;
using MirraCloud.Core.Realtime.Protocol;
using MirraCloud.Core.Realtime.Transport;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using Plugins.MirraCloud.Core.General.LifeCycle;
using UnityEngine.Networking;

namespace MirraCloud.Core.Chats
{
    public class MessageResolveEvent : IRealtimeEvent
    {
    }

    public sealed class ChatsService : ICloudSdkInitializable, ICloudSdkDisposable
    {
        private const string ControllerApi = "/chats/v1/projects";
        private const int MaxHistoryPageSize = 100;
        private const int MaxRecoveryPages = 1000;
        private const int HeartbeatIntervalMs = 15000;

        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly IJsonService _jsonService;
        private readonly IRealtimeAuthProvider _realtimeAuthProvider;
        private readonly IRealtimeSubscriptionStore _subscriptions;
        private readonly IRealtimeConnection _connection;
        private readonly RealtimeReconnectPolicy _reconnectPolicy = new RealtimeReconnectPolicy();
        private readonly Dictionary<string, long> _lastSeenMessageByChannel = new Dictionary<string, long>();

        private bool _shouldBeConnected;
        private bool _reconnectInProgress;
        private CancellationTokenSource _heartbeatCts;

        public event Action<RealtimeConnectionState> OnConnectionStateChanged;
        public event Action<string> OnSubscribed;
        public event Action<ChatMessageDto> OnMessageReceived;
        public event Action<ChatMessageDto> OnMessageEdited;
        public event Action<RealtimeDeletePayload> OnMessageDeleted;
        public event Action<ChatMemberEvent> OnMemberAdded;
        public event Action<ChatMemberEvent> OnMemberRemoved;
        public event Action<string> OnChannelDeleted;
        public event Action<ChatErrorEvent> OnError;

        public ChatsService(
            Configuration configuration,
            ILogger logger,
            RestApiClient restApi,
            IJsonService jsonService)
        {
            _configuration = configuration;
            _logger = logger;
            _restApi = restApi;
            _jsonService = jsonService;

            _realtimeAuthProvider = new ChatsRealtimeAuthProvider(_restApi, _configuration);
            _subscriptions = new RealtimeSubscriptionStore();

            _connection = new RealtimeConnection(
                new ClientWebSocketTransport(),
                new JsonRealtimeSerializer(jsonService),
                new RealtimeRequestTracker(),
                logger);
        }

        public void CloudSdkInitialize()
        {
            _connection.Initialize();

            _connection.OnStateChanged += HandleConnectionStateChanged;
            _connection.OnEvent += HandleRealtimeEvent;
            _connection.OnError += HandleRealtimeError;

            _connection.SubscribeEvent<MessageResolveEvent>("message_resolved", OnMessageResolvedEvent);
        }

        private void OnMessageResolvedEvent(MessageResolveEvent arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void CloudSdkDispose()
        {
            _connection.OnStateChanged -= HandleConnectionStateChanged;
            _connection.OnEvent -= HandleRealtimeEvent;
            _connection.OnError -= HandleRealtimeError;

            _connection.Dispose();

            _shouldBeConnected = false;
            StopHeartbeat();
            _connection.Disconnect();
        }

        public AsyncOperation<RestApiResult<ChatChannelDto>> LookupGroupChannelAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return AsyncOperation<RestApiResult<ChatChannelDto>>.CreateCompleted(
                    RestApiResult<ChatChannelDto>.ValidationFail("groupId is required."));
            }

            var route =
                $"{ControllerApi}/{_configuration.ProjectId}/channels/lookup?ownerRefType=group&ownerRefId={UnityWebRequest.EscapeURL(groupId)}";
            return _restApi.GetAsync<ChatChannelDto>(route);
        }

        public AsyncOperation<RestApiResult<ChatChannelDto>> GetChannelAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult<ChatChannelDto>>.CreateCompleted(
                    RestApiResult<ChatChannelDto>.ValidationFail("channelId is required."));
            }

            var route = $"{ControllerApi}/{_configuration.ProjectId}/channels/{channelId}";
            return _restApi.GetAsync<ChatChannelDto>(route);
        }

        public AsyncOperation<RestApiResult<ChatMemberDto[]>> GetMembersAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult<ChatMemberDto[]>>.CreateCompleted(
                    RestApiResult<ChatMemberDto[]>.ValidationFail("channelId is required."));
            }

            var route = $"{ControllerApi}/{_configuration.ProjectId}/channels/{channelId}/members";
            return _restApi.GetAsync<ChatMemberDto[]>(route);
        }

        public AsyncOperation<RestApiResult<ChatMessageDto[]>> GetMessagesAsync(
            string channelId,
            long? before = null,
            long? after = null,
            int limit = 100,
            string targetMessageId = null)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult<ChatMessageDto[]>>.CreateCompleted(
                    RestApiResult<ChatMessageDto[]>.ValidationFail("channelId is required."));
            }

            var safeLimit = limit < 1 ? 1 : limit > MaxHistoryPageSize ? MaxHistoryPageSize : limit;
            var route =
                $"{ControllerApi}/{_configuration.ProjectId}/channels/{channelId}/messages?limit={safeLimit}";

            if (before.HasValue)
                route += $"&before={before.Value}";

            if (after.HasValue)
                route += $"&after={after.Value}";

            if (!string.IsNullOrWhiteSpace(targetMessageId))
                route += $"&targetId={UnityWebRequest.EscapeURL(targetMessageId)}";

            return _restApi.GetAsync<ChatMessageDto[]>(route);
        }

        public AsyncOperation<RestApiResult> ConnectAsync()
        {
            _shouldBeConnected = true;

            if (_connection.State == RealtimeConnectionState.Connected ||
                _connection.State == RealtimeConnectionState.Connecting)
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.Success());
            }

            if (_reconnectInProgress)
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.Success());
            }

            var op = new AsyncOperation<RestApiResult>();

            var sessionOp = _realtimeAuthProvider.CreateSessionAsync();
            sessionOp.UseCompleted(_ =>
            {
                if (!sessionOp.Result.IsSuccess || sessionOp.Result.Data == null ||
                    string.IsNullOrWhiteSpace(BuildWsUrl(sessionOp.Result.Data)))
                {
                    _shouldBeConnected = false;
                    var error = sessionOp.Result.Error ??
                                RestApiError.Validation("Failed to create realtime session.");
                    op.Complete(RestApiResult.Fail(error).WithMetaFrom(sessionOp.Result));
                    return;
                }

                ConnectInternal(sessionOp.Result.Data, op);
            });

            return op;
        }

        public AsyncOperation<RestApiResult> DisconnectAsync()
        {
            _shouldBeConnected = false;
            _subscriptions.Clear();
            StopHeartbeat();

            var op = new AsyncOperation<RestApiResult>();
            var disconnectOp = _connection.Disconnect();
            disconnectOp.UseCompleted(_ =>
            {
                if (disconnectOp.Result.IsSuccess)
                    op.Complete(RestApiResult.Success());
                else
                    op.Complete(RestApiResult.Fail(
                        RestApiError.Validation(disconnectOp.Result.Message ?? "Disconnect failed")));
            });

            return op;
        }

        public AsyncOperation<RestApiResult> SubscribeAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(
                    RestApiResult.ValidationFail("channelId is required."));
            }

            var op = new AsyncOperation<RestApiResult>();
            var commandOp = _connection.SendCommand("subscribe", new RealtimeAckChannelPayload
            {
                ChannelId = channelId
            });

            commandOp.UseCompleted(_ =>
            {
                if (!commandOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult.Fail(
                        RestApiError.Validation(commandOp.Result.Message ?? "Subscribe failed")));
                    return;
                }

                _subscriptions.Add(channelId);
                op.Complete(RestApiResult.Success());
            });

            return op;
        }

        public AsyncOperation<RestApiResult> UnsubscribeAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(
                    RestApiResult.ValidationFail("channelId is required."));
            }

            var op = new AsyncOperation<RestApiResult>();
            var commandOp = _connection.SendCommand("unsubscribe", new RealtimeAckChannelPayload
            {
                ChannelId = channelId
            });

            commandOp.UseCompleted(_ =>
            {
                if (!commandOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult.Fail(
                        RestApiError.Validation(commandOp.Result.Message ?? "Unsubscribe failed")));
                    return;
                }

                _subscriptions.Remove(channelId);
                op.Complete(RestApiResult.Success());
            });

            return op;
        }

        public AsyncOperation<RestApiResult> JoinAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(
                    RestApiResult.ValidationFail("channelId is required."));
            }

            var route = $"{ControllerApi}/{_configuration.ProjectId}/channels/{channelId}/members";
            return _restApi.PostAsync(route);
        }

        public AsyncOperation<RestApiResult> LeaveAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(
                    RestApiResult.ValidationFail("channelId is required."));
            }

            var profileId = "self";
            var route = $"{ControllerApi}/{_configuration.ProjectId}/channels/{channelId}/members/{profileId}";
            return _restApi.DeleteAsync(route);
        }

        public AsyncOperation<RestApiResult<ChatMessageDto>> SendMessageAsync(
            string channelId,
            string body,
            object metadata = null,
            string[] taggedProfileIds = null,
            string targetMessageId = null)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult<ChatMessageDto>>.CreateCompleted(
                    RestApiResult<ChatMessageDto>.ValidationFail("channelId is required."));
            }

            var op = new AsyncOperation<RestApiResult<ChatMessageDto>>();
            var commandOp = _connection.SendCommand("sendMessage", new
            {
                ChannelId = channelId,
                Body = body,
                Metadata = ToJsonValue(metadata),
                TaggedProfileIds = taggedProfileIds,
                TargetMessageId = targetMessageId
            });

            commandOp.UseCompleted(_ =>
            {
                if (!commandOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult<ChatMessageDto>.Fail(
                        RestApiError.Validation(commandOp.Result.Message ?? "Send failed")));
                    return;
                }

                var dto = JsonValueMapper.Map<ChatMessageDto>(_jsonService, commandOp.Result.Payload);
                TrackLastSeen(dto);
                op.Complete(RestApiResult<ChatMessageDto>.Success(dto));
            });

            return op;
        }

        public AsyncOperation<RestApiResult<ChatMessageDto>> EditMessageAsync(
            string channelId,
            string messageId,
            string body,
            object metadata = null,
            string[] taggedProfileIds = null)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult<ChatMessageDto>>.CreateCompleted(
                    RestApiResult<ChatMessageDto>.ValidationFail("channelId is required."));
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                return AsyncOperation<RestApiResult<ChatMessageDto>>.CreateCompleted(
                    RestApiResult<ChatMessageDto>.ValidationFail("messageId is required."));
            }

            var op = new AsyncOperation<RestApiResult<ChatMessageDto>>();
            var commandOp = _connection.SendCommand("editMessage", new
            {
                ChannelId = channelId,
                MessageId = messageId,
                Body = body,
                Metadata = ToJsonValue(metadata),
                TaggedProfileIds = taggedProfileIds
            });

            commandOp.UseCompleted(_ =>
            {
                if (!commandOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult<ChatMessageDto>.Fail(
                        RestApiError.Validation(commandOp.Result.Message ?? "Edit failed")));
                    return;
                }

                var dto = JsonValueMapper.Map<ChatMessageDto>(_jsonService, commandOp.Result.Payload);
                TrackLastSeen(dto);
                op.Complete(RestApiResult<ChatMessageDto>.Success(dto));
            });

            return op;
        }

        public AsyncOperation<RestApiResult> DeleteMessageAsync(string channelId, string messageId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(
                    RestApiResult.ValidationFail("channelId is required."));
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(
                    RestApiResult.ValidationFail("messageId is required."));
            }

            var op = new AsyncOperation<RestApiResult>();
            var commandOp = _connection.SendCommand("deleteMessage", new
            {
                ChannelId = channelId,
                MessageId = messageId
            });

            commandOp.UseCompleted(_ =>
            {
                if (!commandOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult.Fail(
                        RestApiError.Validation(commandOp.Result.Message ?? "Delete failed")));
                    return;
                }

                op.Complete(RestApiResult.Success());
            });

            return op;
        }

        public AsyncOperation<RestApiResult> MarkAsReadAsync(string channelId, long messageNumber)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(
                    RestApiResult.ValidationFail("channelId is required."));
            }

            if (messageNumber <= 0)
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(
                    RestApiResult.ValidationFail("messageNumber must be greater than 0."));
            }

            var op = new AsyncOperation<RestApiResult>();
            var commandOp = _connection.SendCommand("markAsRead", new
            {
                MessageNumber = messageNumber
            });

            commandOp.UseCompleted(_ =>
            {
                if (!commandOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult.Fail(
                        RestApiError.Validation(commandOp.Result.Message ?? "Mark as read failed")));
                    return;
                }

                op.Complete(RestApiResult.Success());
            });

            return op;
        }

        private void ConnectInternal(RealtimeSessionInfo sessionInfo, AsyncOperation<RestApiResult> operation)
        {
            var wsUrl = BuildWsUrl(sessionInfo);
            if (string.IsNullOrWhiteSpace(wsUrl))
            {
                operation.Complete(RestApiResult.ValidationFail("Realtime wsUrl is empty."));
                return;
            }

            var connectOp = _connection.Connect(wsUrl);
            connectOp.UseCompleted(_ =>
            {
                if (connectOp.Result.IsSuccess)
                    operation.Complete(RestApiResult.Success());
                else
                {
                    _logger.Error($"Chats realtime connect failed: {connectOp.Result.Message}");
                    _shouldBeConnected = false;
                    operation.Complete(
                        RestApiResult.Fail(RestApiError.Validation("Realtime connect failed.")));
                }
            });
        }

        private void HandleConnectionStateChanged(RealtimeConnectionState state)
        {
            OnConnectionStateChanged?.Invoke(state);

            if (state == RealtimeConnectionState.Connected)
            {
                StartHeartbeat();
                return;
            }

            if (state == RealtimeConnectionState.Disconnected && _shouldBeConnected)
            {
                StopHeartbeat();
                _ = StartReconnectLoopAsync();
            }
        }

        private async Task StartReconnectLoopAsync()
        {
            if (_reconnectInProgress)
                return;

            _reconnectInProgress = true;
            OnConnectionStateChanged?.Invoke(RealtimeConnectionState.Reconnecting);
            var reconnected = false;

            try
            {
                for (var attempt = 1; attempt <= _reconnectPolicy.MaxAttempts && _shouldBeConnected; attempt++)
                {
                    var sessionResult = await AwaitOperation(_realtimeAuthProvider.CreateSessionAsync());
                    if (!sessionResult.IsSuccess || sessionResult.Data == null)
                    {
                        await Task.Delay(_reconnectPolicy.GetDelayMs(attempt));
                        continue;
                    }

                    var wsUrl = BuildWsUrl(sessionResult.Data);
                    if (string.IsNullOrWhiteSpace(wsUrl))
                    {
                        await Task.Delay(_reconnectPolicy.GetDelayMs(attempt));
                        continue;
                    }

                    if (!_shouldBeConnected)
                        return;

                    var connectResult = await AwaitOperation(_connection.Connect(wsUrl));
                    if (!connectResult.IsSuccess)
                    {
                        await Task.Delay(_reconnectPolicy.GetDelayMs(attempt));
                        continue;
                    }

                    if (!_shouldBeConnected)
                        return;

                    await RestoreSubscriptionsAndHistoryAsync();
                    reconnected = true;
                    return;
                }
            }
            finally
            {
                _reconnectInProgress = false;

                if (!reconnected && _shouldBeConnected)
                {
                    OnError?.Invoke(new ChatErrorEvent
                    {
                        Code = "reconnect_failed",
                        Message = "Realtime reconnect failed."
                    });
                }
            }
        }

        private async Task RestoreSubscriptionsAndHistoryAsync()
        {
            var channels = _subscriptions.GetAll();
            foreach (var channelId in channels)
            {
                var subscribeResult = await AwaitOperation(
                    _connection.SendCommand("subscribe", new RealtimeAckChannelPayload
                    {
                        ChannelId = channelId
                    }));

                if (!subscribeResult.IsSuccess)
                    continue;

                if (TryGetLastSeen(channelId, out var lastSeenNumber) && lastSeenNumber > 0)
                    await RecoverChannelHistoryAsync(channelId, lastSeenNumber);
            }
        }

        private async Task RecoverChannelHistoryAsync(string channelId, long startAfter)
        {
            var after = startAfter;
            for (var page = 0; page < MaxRecoveryPages; page++)
            {
                if (!_shouldBeConnected || !_connection.IsConnected)
                    return;

                var history =
                    await AwaitOperation(GetMessagesAsync(channelId, after: after, limit: MaxHistoryPageSize));
                if (!history.IsSuccess || history.Data == null || history.Data.Length == 0)
                    return;

                var maxSeen = after;
                foreach (var message in history.Data)
                {
                    TrackLastSeen(message);
                    if (message != null && message.Number > maxSeen)
                        maxSeen = message.Number;

                    OnMessageReceived?.Invoke(message);
                }

                if (maxSeen <= after || history.Data.Length < MaxHistoryPageSize)
                    return;

                after = maxSeen;
            }
        }

        private void HandleRealtimeEvent(RealtimeEvent realtimeEvent)
        {
            switch (realtimeEvent.Name)
            {
                case "subscribed":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload,
                            out RealtimeAckChannelPayload payload))
                        return;

                    if (!string.IsNullOrWhiteSpace(payload?.ChannelId))
                    {
                        _subscriptions.Add(payload.ChannelId);
                        OnSubscribed?.Invoke(payload.ChannelId);
                    }

                    break;
                }
                case "messageCreated":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload, out ChatMessageDto dto))
                        return;

                    TrackLastSeen(dto);
                    OnMessageReceived?.Invoke(dto);
                    break;
                }
                case "messageEdited":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload, out ChatMessageDto dto))
                        return;

                    TrackLastSeen(dto);
                    OnMessageEdited?.Invoke(dto);
                    break;
                }
                case "messageDeleted":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload,
                            out RealtimeDeletePayload payload))
                        return;

                    OnMessageDeleted?.Invoke(payload);
                    break;
                }
                case "memberAdded":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload,
                            out RealtimeMemberPayload payload))
                        return;

                    OnMemberAdded?.Invoke(new ChatMemberEvent
                    {
                        ChannelId = payload.ChannelId,
                        ProfileId = payload.ProfileId
                    });
                    break;
                }
                case "memberRemoved":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload,
                            out RealtimeMemberPayload payload))
                        return;

                    OnMemberRemoved?.Invoke(new ChatMemberEvent
                    {
                        ChannelId = payload.ChannelId,
                        ProfileId = payload.ProfileId
                    });
                    break;
                }
                case "channelDeleted":
                {
                    if (!string.IsNullOrWhiteSpace(realtimeEvent.ChannelId))
                        OnChannelDeleted?.Invoke(realtimeEvent.ChannelId);

                    break;
                }
            }
        }

        private void HandleRealtimeError(RealtimeError realtimeError)
        {
            OnError?.Invoke(new ChatErrorEvent
            {
                Code = realtimeError.Code,
                Message = realtimeError.Message
            });
        }

        private void TrackLastSeen(ChatMessageDto message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.ChannelId))
                return;

            if (!_lastSeenMessageByChannel.TryGetValue(message.ChannelId, out var existing) ||
                message.Number > existing)
            {
                _lastSeenMessageByChannel[message.ChannelId] = message.Number;
            }
        }

        private bool TryGetLastSeen(string channelId, out long number)
        {
            return _lastSeenMessageByChannel.TryGetValue(channelId, out number);
        }

        private JsonValue ToJsonValue(object value)
        {
            if (value == null)
                return new JsonValue(JsonValueType.Null);

            var json = _jsonService.ToJson(value);
            return _jsonService.FromJson<JsonValue>(json);
        }

        private static Task<RealtimeCommandResult> AwaitOperation(
            AsyncOperation<RealtimeCommandResult> operation)
        {
            var tcs = new TaskCompletionSource<RealtimeCommandResult>();
            if (operation.IsDone)
            {
                tcs.TrySetResult(operation.Result);
                return tcs.Task;
            }

            operation.UseCompleted(_ => tcs.TrySetResult(operation.Result));
            return tcs.Task;
        }

        private static Task<RestApiResult<T>> AwaitOperation<T>(AsyncOperation<RestApiResult<T>> operation)
        {
            var tcs = new TaskCompletionSource<RestApiResult<T>>();
            if (operation.IsDone)
            {
                tcs.TrySetResult(operation.Result);
                return tcs.Task;
            }

            operation.UseCompleted(_ => tcs.TrySetResult(operation.Result));
            return tcs.Task;
        }

        private string BuildWsUrl(RealtimeSessionInfo sessionInfo)
        {
            if (sessionInfo == null)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(sessionInfo.WsUrl))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(sessionInfo.RtToken))
                return sessionInfo.WsUrl;

            if (sessionInfo.WsUrl.IndexOf("token=", StringComparison.OrdinalIgnoreCase) >= 0)
                return sessionInfo.WsUrl;

            var separator = sessionInfo.WsUrl.Contains("?") ? "&" : "?";
            return $"{sessionInfo.WsUrl}{separator}token={UnityWebRequest.EscapeURL(sessionInfo.RtToken)}";
        }

        private void StartHeartbeat()
        {
            if (_heartbeatCts != null)
                return;

            _heartbeatCts = new CancellationTokenSource();
            _ = RunHeartbeatLoopAsync(_heartbeatCts.Token);
        }

        private void StopHeartbeat()
        {
            if (_heartbeatCts == null)
                return;

            var cts = _heartbeatCts;
            _heartbeatCts = null;

            try
            {
                cts.Cancel();
            }
            catch
            {
            }
            finally
            {
                cts.Dispose();
            }
        }

        private async Task RunHeartbeatLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _shouldBeConnected)
            {
                try
                {
                    await Task.Delay(HeartbeatIntervalMs, token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (!_connection.IsConnected)
                    return;

                try
                {
                    var result = await AwaitOperation(
                        _connection.SendCommand("ping", new { }, 5000));
                    if (!result.IsSuccess &&
                        string.Equals(result.Code, "not_connected", StringComparison.Ordinal))
                    {
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(new ChatErrorEvent
                    {
                        Code = "heartbeat_failed",
                        Message = ex.Message
                    });
                }
            }
        }
    }
}
