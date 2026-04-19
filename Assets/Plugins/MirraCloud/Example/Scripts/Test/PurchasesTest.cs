using System.Linq;
using MirraCloud.Core;
using MirraCloud.Core.Purchases;
using MirraCloud.Core.Purchases.Models;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class PurchasesTest : MonoBehaviour
    {
        [SerializeField] private string _purchaseKey;
        [SerializeField] private string _providerConfigId;

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        private void Start()
        {
            _sdk.Purchases.OnPurchaseCompleted += order =>
                Debug.Log($"[Purchases] Completed order {order.OrderId}, amount={order.Amount} {order.Currency}");

            _sdk.Purchases.OnPurchaseCancelled += operationId =>
                Debug.Log($"[Purchases] Cancelled {operationId}");

            _sdk.Purchases.OnPurchaseFailed += result =>
                Debug.LogWarning($"[Purchases] Failed {result.OperationId}: {result.Error}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) LoadCatalog();
            if (Input.GetKeyDown(KeyCode.Alpha2)) Buy();
            if (Input.GetKeyDown(KeyCode.Alpha3)) GetOrders();
            if (Input.GetKeyDown(KeyCode.Alpha4)) GetSubscriptions();
        }

        public void LoadCatalog()
        {
            var op = _sdk.Purchases.LoadCatalogAsync();
            op.UseCompleted(_ =>
            {
                if (!op.Result.IsSuccess || op.Result.Data == null)
                {
                    Debug.LogError($"[Purchases] LoadCatalog failed: {op.Result.Error?.Message}");
                    return;
                }

                Debug.Log($"[Purchases] Catalog: {op.Result.Data.Count} items");
                foreach (var item in op.Result.Data)
                {
                    var priceList = item.Prices != null
                        ? string.Join(", ", item.Prices.Select(p => $"{p.Amount} {p.Currency} via {p.ProviderName}"))
                        : "no prices";
                    Debug.Log($"  - {item.Key} ({item.Type}): {item.DisplayName} [{priceList}]");
                }
            });
        }

        public void Buy()
        {
            if (string.IsNullOrWhiteSpace(_purchaseKey) || string.IsNullOrWhiteSpace(_providerConfigId))
            {
                Debug.LogError("[Purchases] _purchaseKey and _providerConfigId must be set in the inspector.");
                return;
            }

            var op = _sdk.Purchases.BuyAsync(_purchaseKey, _providerConfigId);
            op.UseCompleted(_ =>
            {
                var result = op.Result;
                Debug.Log($"[Purchases] BuyAsync finished: status={result.Status}, operationId={result.OperationId}, error={result.Error}");
            });
        }

        public void GetOrders()
        {
            var op = _sdk.Purchases.GetOrdersAsync();
            op.UseCompleted(_ =>
            {
                if (!op.Result.IsSuccess || op.Result.Data == null)
                {
                    Debug.LogError($"[Purchases] GetOrders failed: {op.Result.Error?.Message}");
                    return;
                }

                Debug.Log($"[Purchases] Orders: {op.Result.Data.Count}");
                foreach (var order in op.Result.Data)
                {
                    Debug.Log($"  - {order.OrderId} [{order.Status}] {order.Amount} {order.Currency}");
                }
            });
        }

        public void GetSubscriptions()
        {
            var op = _sdk.Purchases.GetSubscriptionsAsync();
            op.UseCompleted(_ =>
            {
                if (!op.Result.IsSuccess || op.Result.Data == null)
                {
                    Debug.LogError($"[Purchases] GetSubscriptions failed: {op.Result.Error?.Message}");
                    return;
                }

                Debug.Log($"[Purchases] Subscriptions: {op.Result.Data.Count}");
                foreach (var sub in op.Result.Data)
                {
                    Debug.Log($"  - {sub.SubscriptionId} [{sub.Status}] until {sub.CurrentPeriodEnd}");
                }
            });
        }
    }
}
