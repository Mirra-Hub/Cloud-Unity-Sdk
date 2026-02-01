using System;
using System.Collections.Generic;
using MirraCloud.Core.Economy.Dto;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace MirraCloud.Core.Economy
{
    public sealed class EconomyService
    {
        private const string ControllerApi = "/economy/v1/projects";

        private readonly ILogger _logger;
        private readonly Configuration _configuration;
        private readonly RestApiClient _restApi;

        private readonly Dictionary<string, EconomyResourceConfig> _currencies = new(StringComparer.Ordinal);
        private readonly Dictionary<string, EconomyResourceConfig> _items = new(StringComparer.Ordinal);
        private readonly Dictionary<string, EconomyResourceConfig> _containers = new(StringComparer.Ordinal);
        private readonly Dictionary<string, EconomyResourceConfig> _lootboxes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, EconomyResourceConfig> _resourcesByKey = new(StringComparer.Ordinal);

        public IReadOnlyDictionary<string, EconomyResourceConfig> Currencies => _currencies;
        public IReadOnlyDictionary<string, EconomyResourceConfig> Items => _items;
        public IReadOnlyDictionary<string, EconomyResourceConfig> Containers => _containers;
        public IReadOnlyDictionary<string, EconomyResourceConfig> Lootboxes => _lootboxes;

        public EconomyService(Configuration configuration, ILogger logger, RestApiClient restApi)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
        }

        public AsyncOperation<RestApiResult<EconomyConfigsDto>> LoadConfigsAsync()
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/configs";
            var operation = _restApi.GetAsync<EconomyConfigsDto>(route);

            operation.UseCompleted(_ =>
            {
                if (!operation.Result.IsSuccess || operation.Result.Data == null)
                {
                    return;
                }

                ApplyConfigs(operation.Result.Data);
            });

            return operation;
        }

        public AsyncOperation<RestApiResult<PlayerInventoryDto>> LoadInventoryAsync()
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/inventories";
            return _restApi.GetAsync<PlayerInventoryDto>(route);
        }

        public AsyncOperation<RestApiResult<PlayerInventoryDto>> GrantAsync(RewardDto[] rewards)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/inventories/grant";
            var dto = new GrantRewardsDto { Rewards = rewards ?? Array.Empty<RewardDto>() };
            return _restApi.PostAsync<PlayerInventoryDto>(route, dto);
        }

        public AsyncOperation<RestApiResult<PlayerInventoryDto>> AddCurrencyAsync(string key, decimal amount)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/inventories/currencies/add";
            var dto = new ModifyResourceAmountDto { Key = key, Amount = amount };
            return _restApi.PostAsync<PlayerInventoryDto>(route, dto);
        }

        public AsyncOperation<RestApiResult<PlayerInventoryDto>> SpendCurrencyAsync(string key, decimal amount)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/inventories/currencies/spend";
            var dto = new ModifyResourceAmountDto { Key = key, Amount = amount };
            return _restApi.PostAsync<PlayerInventoryDto>(route, dto);
        }

        public AsyncOperation<RestApiResult<PlayerInventoryDto>> AddItemAsync(string key, decimal amount)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/inventories/items/add";
            var dto = new ModifyResourceAmountDto { Key = key, Amount = amount };
            return _restApi.PostAsync<PlayerInventoryDto>(route, dto);
        }

        public AsyncOperation<RestApiResult<PlayerInventoryDto>> SpendItemAsync(string key, decimal amount)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/inventories/items/spend";
            var dto = new ModifyResourceAmountDto { Key = key, Amount = amount };
            return _restApi.PostAsync<PlayerInventoryDto>(route, dto);
        }

        public AsyncOperation<RestApiResult<InventoryWithRewardsDto>> ConsumeContainerAsync(string key)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/inventories/containers/consume";
            var dto = new ConsumeContainerDto { Key = key };
            return _restApi.PostAsync<InventoryWithRewardsDto>(route, dto);
        }

        public AsyncOperation<RestApiResult<InventoryWithRewardsDto>> OpenLootboxAsync(string key)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/inventories/lootboxes/open";
            var dto = new OpenLootboxDto { Key = key };
            return _restApi.PostAsync<InventoryWithRewardsDto>(route, dto);
        }

        private void ApplyConfigs(EconomyConfigsDto dto)
        {
            ClearCache();
            AddResources(dto.Currencies, EconomyResourceKind.Currency, _currencies);
            AddResources(dto.Items, EconomyResourceKind.Item, _items);
            AddResources(dto.Containers, EconomyResourceKind.Container, _containers);
            AddResources(dto.Lootboxes, EconomyResourceKind.Lootbox, _lootboxes);
        }

        public void ClearCache()
        {
            _currencies.Clear();
            _items.Clear();
            _containers.Clear();
            _lootboxes.Clear();
            _resourcesByKey.Clear();
        }

        private void AddResources(
            Dictionary<string, EconomySdkResourceDto> source,
            EconomyResourceKind kind,
            Dictionary<string, EconomyResourceConfig> target)
        {
            if (source == null)
            {
                return;
            }

            foreach (var pair in source)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
                {
                    continue;
                }

                var fields = pair.Value.Fields ?? new JsonValue(JsonValueType.Object);
                var components = pair.Value.Components ?? new Dictionary<string, JsonValue>(StringComparer.Ordinal);

                var resource = new EconomyResourceConfig(pair.Key, kind, fields, components);
                target[pair.Key] = resource;
                _resourcesByKey[pair.Key] = resource;
            }
        }

        public bool TryGetResource(string key, out EconomyResourceConfig resource)
        {
            resource = null;
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return _resourcesByKey.TryGetValue(key, out resource);
        }

        public T GetResourceFields<T>(string key)
        {
            if (TryGetResourceFields<T>(key, out var fields))
            {
                return fields;
            }

            throw new InvalidOperationException($"Economy resource '{key}' is missing or cannot be mapped to '{typeof(T).Name}'.");
        }

        public bool TryGetResourceFields<T>(string key, out T fields)
        {
            fields = default;
            if (!TryGetResource(key, out var resource) || resource.Fields == null)
            {
                return false;
            }

            return JsonValueMapper.TryMap(_restApi.JsonService, resource.Fields, out fields);
        }

        public T GetResourceComponent<T>(string key, string componentKey)
        {
            if (TryGetResourceComponent<T>(key, componentKey, out var component))
            {
                return component;
            }

            throw new InvalidOperationException(
                $"Economy resource '{key}' component '{componentKey}' is missing or cannot be mapped to '{typeof(T).Name}'.");
        }

        public bool TryGetResourceComponent<T>(string key, string componentKey, out T component)
        {
            component = default;
            if (string.IsNullOrWhiteSpace(componentKey))
            {
                return false;
            }

            if (!TryGetResource(key, out var resource))
            {
                return false;
            }

            if (resource.Components == null || !resource.Components.TryGetValue(componentKey, out var data) || data == null)
            {
                return false;
            }

            return JsonValueMapper.TryMap(_restApi.JsonService, data, out component);
        }
    }

    public enum EconomyResourceKind
    {
        Currency = 0,
        Item = 1,
        Container = 2,
        Lootbox = 3
    }

    public sealed class EconomyResourceConfig
    {
        public string Key { get; }
        public EconomyResourceKind Kind { get; }
        public JsonValue Fields { get; }
        public IReadOnlyDictionary<string, JsonValue> Components { get; }

        public EconomyResourceConfig(
            string key,
            EconomyResourceKind kind,
            JsonValue fields,
            IReadOnlyDictionary<string, JsonValue> components)
        {
            Key = key;
            Kind = kind;
            Fields = fields;
            Components = components;
        }
    }
}
