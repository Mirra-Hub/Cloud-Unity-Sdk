using System.Collections.Generic;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace MirraCloud.Core.Economy
{
    public class EconomyService
    {
        private readonly ILogger _logger;
        private readonly Configuration _configuration;
        private readonly RestApiClient _restApi;

        private const string ControllerApi = "/economy/v1/projects";

        private readonly List<CurrencyEconomyDefinition> _currencies = new List<CurrencyEconomyDefinition>();
        private readonly List<ItemEconomyDefinition> _items = new List<ItemEconomyDefinition>();
        private readonly List<TradeEconomyDefinition> _trades = new List<TradeEconomyDefinition>();

        public IReadOnlyCollection<CurrencyEconomyDefinition> Currencies => _currencies;
        public IReadOnlyCollection<ItemEconomyDefinition> Items => _items;
        public IReadOnlyCollection<TradeEconomyDefinition> Trades => _trades;

        public EconomyService(Configuration configuration, ILogger logger, RestApiClient restApi)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
        }

        public AsyncOperation<RestApiResult<EconomyConfigDto>> LoadConfigAsync()
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/configs";

            var operation = _restApi.GetAsync<EconomyConfigDto>(route);

            _currencies.Clear();
            _items.Clear();
            _trades.Clear();

            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess && completed.Result.Data != null)
                {
                    var config = completed.Result.Data;

                    Debug.Log(JsonUtility.ToJson(config));

                    foreach (var currencyDefinitionDto in config.currencies)
                    {
                        _currencies.Add(new CurrencyEconomyDefinition(currencyDefinitionDto));
                    }

                    foreach (var itemDefinitionDto in config.items)
                    {
                        _items.Add(new ItemEconomyDefinition(itemDefinitionDto));
                    }
                }
            };

            return operation;
        }

        public AsyncOperation<RestApiResult<ResultCurrencyOperationDto>> AddCurrencyAsync(string currencyId, int amount)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/add";
            var dto = new ChangeCurrencyOperationDto { CurrencyId = currencyId, Amount = amount };
            return _restApi.PatchAsync<ResultCurrencyOperationDto>(route, dto);
        }

        public AsyncOperation<RestApiResult<ResultCurrencyOperationDto>> SubtractCurrencyAsync(string currencyId, int amount)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/subtract";
            var dto = new ChangeCurrencyOperationDto { CurrencyId = currencyId, Amount = amount };
            return _restApi.PatchAsync<ResultCurrencyOperationDto>(route, dto);
        }

        public AsyncOperation<RestApiResult<ResultCurrencyOperationDto>> SetCurrencyAsync(string currencyId, int amount)
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/branches/{_configuration.BranchId}/set";
            var dto = new ChangeCurrencyOperationDto { CurrencyId = currencyId, Amount = amount };
            return _restApi.PutAsync<ResultCurrencyOperationDto>(route, dto);
        }
    }
}

