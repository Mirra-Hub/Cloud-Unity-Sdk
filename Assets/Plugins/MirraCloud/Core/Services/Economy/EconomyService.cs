using System.Collections.Generic;
using UnityEngine;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace MirraCloud.Core.Economy
{
    public class EconomyService 
    {
        private readonly ILogger _logger;
        private readonly Configuration _configuration;
        private readonly RestApiClient _restApi;

        private const string ControllerApi = "/economy/v1";

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

        public IRestApiOperation LoadConfigAsync()
        {
            string route = $"{ControllerApi}/{_configuration.ProjectId}/config/";
            
            var operation = _restApi.Get(route);

            _currencies.Clear();
            _items.Clear();
            _trades.Clear();
            
            operation.UseCompletedCallback(requestOperation =>
            {
                if (operation.IsSuccess)
                {
                    Debug.Log(requestOperation.DownloadHandler.text);
                    
                    var config = requestOperation.GetData<EconomyConfigDto>();
                    
                    Debug.Log(JsonUtility.ToJson(config));
                    
                    foreach (var currencyDefinitionDto in config.currencies)
                    {
                        _currencies.Add(new CurrencyEconomyDefinition(currencyDefinitionDto));
                    }

                    foreach (var itemDefinitionDto in config.items)
                    {
                        _items.Add(new ItemEconomyDefinition(itemDefinitionDto));
                    }   
                    
                    foreach (var tradeDefinitionDto in config.virtualPurchases)
                    {
                        _trades.Add(new TradeEconomyDefinition(tradeDefinitionDto));
                    }
                }
            });

            return operation;
        }
        
        public IRestApiOperation<ResultCurrencyOperationDto> AddCurrencyAsync(string currencyId, int amount)
        {
            ChangeCurrencyOperationDto operationDto = new ChangeCurrencyOperationDto()
            {
                CurrencyId = currencyId,
                Amount = amount,
            };

            string route = $"{ControllerApi}/{_configuration.ProjectId}/currency/";
            
            var operation = _restApi.Put<ResultCurrencyOperationDto>(route, operationDto);

            operation.UseExtractData(apiOperation => operation.GetData<ResultCurrencyOperationDto>());
            
            return operation;
        }

        public IRestApiOperation<ResultCurrencyOperationDto> SubtractCurrencyAsync(string currencyId, int amount)
        {
            ChangeCurrencyOperationDto operationDto = new ChangeCurrencyOperationDto()
            {
                CurrencyId = currencyId,
                Amount = amount,
            };

            string route = $"{ControllerApi}/{_configuration.ProjectId}/currency/";
            
            var operation = _restApi.Put<ResultCurrencyOperationDto>(route, operationDto);

            operation.UseExtractData(apiOperation => operation.GetData<ResultCurrencyOperationDto>());
            
            return operation;
        }

        public IRestApiOperation<ResultCurrencyOperationDto> SetCurrencyAsync(string currencyId, int amount)
        {
            ChangeCurrencyOperationDto operationDto = new ChangeCurrencyOperationDto()
            {
                CurrencyId = currencyId,
                Amount = amount,
            };

            string route = $"{ControllerApi}/{_configuration.ProjectId}/currency/";
            
            var operation = _restApi.Post<ResultCurrencyOperationDto>(route, operationDto);

            operation.UseExtractData(apiOperation => operation.GetData<ResultCurrencyOperationDto>());
            
            return operation;
        }
    }
}