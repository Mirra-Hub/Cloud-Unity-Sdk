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
using MirraCloud.Core.Realtime.Dispatching;
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
    
    // TODO: create interface IChatService with public API to developers
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
        private readonly MainThreadDispatcher _dispatcher;
        private readonly RealtimeReconnectPolicy _reconnectPolicy = new RealtimeReconnectPolicy();
        private readonly Dictionary<string, long> _lastSeenMessageByChannel = new Dictionary<string, long>();
        private readonly object _lastSeenSync = new object();
        private readonly object _heartbeatSync = new object();

        private bool _shouldBeConnected;
        private int _reconnectInProgress;
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

            _dispatcher = new MainThreadDispatcher(SynchronizationContext.Current);
            _connection = new RealtimeConnection(
                new ClientWebSocketTransport(),
                new JsonRealtimeSerializer(jsonService),
                new RealtimeRequestTracker(),
                _dispatcher,
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
            _ = _connection.DisconnectAsync();
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
            var route = $"{ControllerApi}/{_configuration.ProjectId}/channels/{channelId}/messages?limit={safeLimit}";

            if (before.HasValue)
            {
                route += $"&before={before.Value}";
            }

            if (after.HasValue)
            {
                route += $"&after={after.Value}";
            }

            if (!string.IsNullOrWhiteSpace(targetMessageId))
            {
                route += $"&targetId={UnityWebRequest.EscapeURL(targetMessageId)}";
            }

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

            if (Interlocked.CompareExchange(ref _reconnectInProgress, 0, 0) == 1)
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
                    var error = sessionOp.Result.Error ?? RestApiError.Validation("Failed to create realtime session.");
                    CompleteOnMainThread(op, RestApiResult.Fail(error).WithMetaFrom(sessionOp.Result));
                    return;
                }

                ConnectInternalAsync(sessionOp.Result.Data, op);
            });

            return op;
        }

        public AsyncOperation<RestApiResult> DisconnectAsync()
        {
            var op = new AsyncOperation<RestApiResult>();
            _shouldBeConnected = false;
            _subscriptions.Clear();
            StopHeartbeat();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _connection.DisconnectAsync();
                    CompleteOnMainThread(op, RestApiResult.Success());
                }
                catch (Exception ex)
                {
                    CompleteOnMainThread(op, RestApiResult.Fail(RestApiError.Validation(ex.Message)));
                }
            });

            return op;
        }

        public AsyncOperation<RestApiResult> SubscribeAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.ValidationFail("channelId is required."));
            }

            var op = new AsyncOperation<RestApiResult>();
            _ = Task.Run(async () =>
            {
                try
                {
                    var command = await _connection.SendCommandAsync("subscribe", new RealtimeAckChannelPayload
                    {
                        ChannelId = channelId
                    });

                    if (!command.IsSuccess)
                    {
                        CompleteOnMainThread(op, RestApiResult.Fail(RestApiError.Validation(command.Message ?? "Subscribe failed")));
                        return;
                    }

                    _subscriptions.Add(channelId);
                    CompleteOnMainThread(op, RestApiResult.Success());
                }
                catch (Exception ex)
                {
                    CompleteOnMainThread(op, RestApiResult.Fail(RestApiError.Validation(ex.Message)));
                }
            });

            return op;
        }

        public AsyncOperation<RestApiResult> UnsubscribeAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.ValidationFail("channelId is required."));
            }

            var op = new AsyncOperation<RestApiResult>();
            _ = Task.Run(async () =>
            {
                try
                {
                    var command = await _connection.SendCommandAsync("unsubscribe", new RealtimeAckChannelPayload
                    {
                        ChannelId = channelId
                    });

                    if (!command.IsSuccess)
                    {
                        CompleteOnMainThread(op, RestApiResult.Fail(RestApiError.Validation(command.Message ?? "Unsubscribe failed")));
                        return;
                    }

                    _subscriptions.Remove(channelId);
                    CompleteOnMainThread(op, RestApiResult.Success());
                }
                catch (Exception ex)
                {
                    CompleteOnMainThread(op, RestApiResult.Fail(RestApiError.Validation(ex.Message)));
                }
            });

            return op;
        }

        public AsyncOperation<RestApiResult> JoinAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.ValidationFail("channelId is required."));
            }

            var route = $"{ControllerApi}/{_configuration.ProjectId}/channels/{channelId}/members";
            return _restApi.PostAsync(route);
        }

        public AsyncOperation<RestApiResult> LeaveAsync(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.ValidationFail("channelId is required."));
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
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await _connection.SendCommandAsync("sendMessage", new
                    {
                        ChannelId = channelId,
                        Body = body,
                        Metadata = ToJsonValue(metadata),
                        TaggedProfileIds = taggedProfileIds,
                        TargetMessageId = targetMessageId
                    });

                    if (!result.IsSuccess)
                    {
                        CompleteOnMainThread(op, RestApiResult<ChatMessageDto>.Fail(
                            RestApiError.Validation(result.Message ?? "Send failed")));
                        return;
                    }

                    var dto = JsonValueMapper.Map<ChatMessageDto>(_jsonService, result.Payload);
                    TrackLastSeen(dto);
                    CompleteOnMainThread(op, RestApiResult<ChatMessageDto>.Success(dto));
                }
                catch (Exception ex)
                {
                    CompleteOnMainThread(op, RestApiResult<ChatMessageDto>.Fail(RestApiError.Validation(ex.Message)));
                }
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
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await _connection.SendCommandAsync("editMessage", new
                    {
                        ChannelId = channelId,
                        MessageId = messageId,
                        Body = body,
                        Metadata = ToJsonValue(metadata),
                        TaggedProfileIds = taggedProfileIds
                    });

                    if (!result.IsSuccess)
                    {
                        CompleteOnMainThread(op, RestApiResult<ChatMessageDto>.Fail(
                            RestApiError.Validation(result.Message ?? "Edit failed")));
                        return;
                    }

                    var dto = JsonValueMapper.Map<ChatMessageDto>(_jsonService, result.Payload);
                    TrackLastSeen(dto);
                    CompleteOnMainThread(op, RestApiResult<ChatMessageDto>.Success(dto));
                }
                catch (Exception ex)
                {
                    CompleteOnMainThread(op, RestApiResult<ChatMessageDto>.Fail(RestApiError.Validation(ex.Message)));
                }
            });

            return op;
        }

        public AsyncOperation<RestApiResult> DeleteMessageAsync(string channelId, string messageId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.ValidationFail("channelId is required."));
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.ValidationFail("messageId is required."));
            }

            var op = new AsyncOperation<RestApiResult>();
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await _connection.SendCommandAsync("deleteMessage", new
                    {
                        ChannelId = channelId,
                        MessageId = messageId
                    });

                    if (!result.IsSuccess)
                    {
                        CompleteOnMainThread(op, RestApiResult.Fail(RestApiError.Validation(result.Message ?? "Delete failed")));
                        return;
                    }

                    CompleteOnMainThread(op, RestApiResult.Success());
                }
                catch (Exception ex)
                {
                    CompleteOnMainThread(op, RestApiResult.Fail(RestApiError.Validation(ex.Message)));
                }
            });

            return op;
        }

        private async Task ConnectInternalAsync(RealtimeSessionInfo sessionInfo, AsyncOperation<RestApiResult> operation)
        {
            try
            {
                var wsUrl = BuildWsUrl(sessionInfo);
                if (string.IsNullOrWhiteSpace(wsUrl))
                {
                    CompleteOnMainThread(operation, RestApiResult.ValidationFail("Realtime wsUrl is empty."));
                    return;
                }

                await _connection.ConnectAsync(wsUrl);
                CompleteOnMainThread(operation, RestApiResult.Success());
            }
            catch (Exception ex)
            {
                _logger.Error($"Chats realtime connect failed: {ex.Message}");
                _shouldBeConnected = false;
                CompleteOnMainThread(operation, RestApiResult.Fail(RestApiError.Validation("Realtime connect failed.")));
            }
        }

        private void HandleConnectionStateChanged(RealtimeConnectionState state)
        {
            EmitConnectionState(state);

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
            if (Interlocked.CompareExchange(ref _reconnectInProgress, 1, 0) != 0)
            {
                return;
            }

            EmitConnectionState(RealtimeConnectionState.Reconnecting);
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
                    {
                        return;
                    }

                    try
                    {
                        await _connection.ConnectAsync(wsUrl);
                    }
                    catch
                    {
                        await Task.Delay(_reconnectPolicy.GetDelayMs(attempt));
                        continue;
                    }

                    if (!_shouldBeConnected)
                    {
                        return;
                    }

                    await RestoreSubscriptionsAndHistoryAsync();
                    reconnected = true;
                    return;
                }
            }
            finally
            {
                Interlocked.Exchange(ref _reconnectInProgress, 0);

                if (!reconnected && _shouldBeConnected)
                {
                    EmitError(new ChatErrorEvent
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
                var subscribeResult = await _connection.SendCommandAsync("subscribe", new RealtimeAckChannelPayload
                {
                    ChannelId = channelId
                });

                if (!subscribeResult.IsSuccess)
                {
                    continue;
                }

                if (TryGetLastSeen(channelId, out var lastSeenNumber) && lastSeenNumber > 0)
                {
                    await RecoverChannelHistoryAsync(channelId, lastSeenNumber);
                }
            }
        }

        private async Task RecoverChannelHistoryAsync(string channelId, long startAfter)
        {
            var after = startAfter;
            for (var page = 0; page < MaxRecoveryPages; page++)
            {
                if (!_shouldBeConnected || !_connection.IsConnected)
                {
                    return;
                }

                var history = await AwaitOperation(GetMessagesAsync(channelId, after: after, limit: MaxHistoryPageSize));
                if (!history.IsSuccess || history.Data == null || history.Data.Length == 0)
                {
                    return;
                }

                var maxSeen = after;
                foreach (var message in history.Data)
                {
                    TrackLastSeen(message);
                    if (message != null && message.Number > maxSeen)
                    {
                        maxSeen = message.Number;
                    }

                    EmitMessageReceived(message);
                }

                if (maxSeen <= after || history.Data.Length < MaxHistoryPageSize)
                {
                    return;
                }

                after = maxSeen;
            }
        }

        private void HandleRealtimeEvent(RealtimeEvent realtimeEvent)
        {
            switch (realtimeEvent.Name)
            {
                case "subscribed":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload, out RealtimeAckChannelPayload payload))
                    {
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(payload?.ChannelId))
                    {
                        _subscriptions.Add(payload.ChannelId);
                        EmitSubscribed(payload.ChannelId);
                    }

                    break;
                }
                case "messageCreated":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload, out ChatMessageDto dto))
                    {
                        return;
                    }

                    TrackLastSeen(dto);
                    EmitMessageReceived(dto);
                    break;
                }
                case "messageEdited":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload, out ChatMessageDto dto))
                    {
                        return;
                    }

                    TrackLastSeen(dto);
                    EmitMessageEdited(dto);
                    break;
                }
                case "messageDeleted":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload, out RealtimeDeletePayload payload))
                    {
                        return;
                    }

                    EmitMessageDeleted(payload);
                    break;
                }
                case "memberAdded":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload, out RealtimeMemberPayload payload))
                    {
                        return;
                    }

                    EmitMemberAdded(new ChatMemberEvent
                    {
                        ChannelId = payload.ChannelId,
                        ProfileId = payload.ProfileId
                    });
                    break;
                }
                case "memberRemoved":
                {
                    if (!JsonValueMapper.TryMap(_jsonService, realtimeEvent.Payload, out RealtimeMemberPayload payload))
                    {
                        return;
                    }

                    EmitMemberRemoved(new ChatMemberEvent
                    {
                        ChannelId = payload.ChannelId,
                        ProfileId = payload.ProfileId
                    });
                    break;
                }
                case "channelDeleted":
                {
                    if (!string.IsNullOrWhiteSpace(realtimeEvent.ChannelId))
                    {
                        EmitChannelDeleted(realtimeEvent.ChannelId);
                    }

                    break;
                }
            }
        }

        private void HandleRealtimeError(RealtimeError realtimeError)
        {
            EmitError(new ChatErrorEvent
            {
                Code = realtimeError.Code,
                Message = realtimeError.Message
            });
        }

        private void TrackLastSeen(ChatMessageDto message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.ChannelId))
            {
                return;
            }

            lock (_lastSeenSync)
            {
                if (!_lastSeenMessageByChannel.TryGetValue(message.ChannelId, out var existing) || message.Number > existing)
                {
                    _lastSeenMessageByChannel[message.ChannelId] = message.Number;
                }
            }
        }

        private bool TryGetLastSeen(string channelId, out long number)
        {
            lock (_lastSeenSync)
            {
                return _lastSeenMessageByChannel.TryGetValue(channelId, out number);
            }
        }

        private JsonValue ToJsonValue(object value)
        {
            if (value == null)
            {
                return new JsonValue(JsonValueType.Null);
            }

            var json = _jsonService.ToJson(value);
            return _jsonService.FromJson<JsonValue>(json);
        }

        private static Task<RestApiResult<T>> AwaitOperation<T>(AsyncOperation<RestApiResult<T>> operation)
        {
            var tcs = new TaskCompletionSource<RestApiResult<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (operation.IsDone)
            {
                tcs.TrySetResult(operation.Result);
                return tcs.Task;
            }

            operation.UseCompleted(_ => tcs.TrySetResult(operation.Result));
            return tcs.Task;
        }

        private void EmitConnectionState(RealtimeConnectionState state)
        {
            _dispatcher.Post(() => OnConnectionStateChanged?.Invoke(state));
        }

        private void EmitSubscribed(string channelId)
        {
            _dispatcher.Post(() => OnSubscribed?.Invoke(channelId));
        }

        private void EmitMessageReceived(ChatMessageDto message)
        {
            _dispatcher.Post(() => OnMessageReceived?.Invoke(message));
        }

        private void EmitMessageEdited(ChatMessageDto message)
        {
            _dispatcher.Post(() => OnMessageEdited?.Invoke(message));
        }

        private void EmitMessageDeleted(RealtimeDeletePayload payload)
        {
            _dispatcher.Post(() => OnMessageDeleted?.Invoke(payload));
        }

        private void EmitMemberAdded(ChatMemberEvent payload)
        {
            _dispatcher.Post(() => OnMemberAdded?.Invoke(payload));
        }

        private void EmitMemberRemoved(ChatMemberEvent payload)
        {
            _dispatcher.Post(() => OnMemberRemoved?.Invoke(payload));
        }

        private void EmitChannelDeleted(string channelId)
        {
            _dispatcher.Post(() => OnChannelDeleted?.Invoke(channelId));
        }

        private void EmitError(ChatErrorEvent error)
        {
            _dispatcher.Post(() => OnError?.Invoke(error));
        }

        private void CompleteOnMainThread(AsyncOperation<RestApiResult> operation, RestApiResult result)
        {
            _dispatcher.Post(() => operation.Complete(result));
        }

        private void CompleteOnMainThread<T>(AsyncOperation<RestApiResult<T>> operation, RestApiResult<T> result)
        {
            _dispatcher.Post(() => operation.Complete(result));
        }

        private string BuildWsUrl(RealtimeSessionInfo sessionInfo)
        {
            if (sessionInfo == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(sessionInfo.WsUrl))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(sessionInfo.RtToken))
            {
                return sessionInfo.WsUrl;
            }

            if (sessionInfo.WsUrl.IndexOf("token=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return sessionInfo.WsUrl;
            }

            var separator = sessionInfo.WsUrl.Contains("?") ? "&" : "?";
            return $"{sessionInfo.WsUrl}{separator}token={UnityWebRequest.EscapeURL(sessionInfo.RtToken)}";
        }

        private void StartHeartbeat()
        {
            lock (_heartbeatSync)
            {
                if (_heartbeatCts != null)
                {
                    return;
                }

                _heartbeatCts = new CancellationTokenSource();
                _ = RunHeartbeatLoopAsync(_heartbeatCts.Token);
            }
        }

        private void StopHeartbeat()
        {
            CancellationTokenSource cts = null;

            lock (_heartbeatSync)
            {
                if (_heartbeatCts == null)
                {
                    return;
                }

                cts = _heartbeatCts;
                _heartbeatCts = null;
            }

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
                {
                    return;
                }

                try
                {
                    var result = await _connection.SendCommandAsync("ping", new { }, 5000, token);
                    if (!result.IsSuccess && string.Equals(result.Code, "not_connected", StringComparison.Ordinal))
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
                    EmitError(new ChatErrorEvent
                    {
                        Code = "heartbeat_failed",
                        Message = ex.Message
                    });
                }
            }
        }
    }
}
