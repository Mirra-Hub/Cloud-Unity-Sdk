using MirraCloud.Core;
using MirraCloud.Core.PromoCodes.Enums;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    /// <summary>
    /// Demo for the PromoCodes SDK module. Wire the script onto a scene object and
    /// type in a promo code, then press the keyboard shortcuts to exercise endpoints.
    /// </summary>
    public class PromoCodesTest : MonoBehaviour
    {
        [SerializeField] private string _code;

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Redeem();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GetHistory();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GetActiveEffects();
            }
        }

        public void Redeem()
        {
            var operation = _sdk.PromoCodes.RedeemAsync(_code);
            operation.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[PromoCodes] Redeem failed: {completed.Result}");
                    return;
                }

                var data = completed.Result.Data;
                if (data.status == RedemptionStatus.Success)
                {
                    Debug.Log($"[PromoCodes] Redeemed campaign '{data.campaignKey}' " +
                              $"({data.campaignDisplayName}). Rewards: {data.rewards.Count}, " +
                              $"effects: {data.effects.Count}.");
                }
                else
                {
                    Debug.LogWarning($"[PromoCodes] Redeem rejected with status code {data.status} " +
                                     $"for code '{_code}'.");
                }
            };
        }

        public void GetHistory()
        {
            var operation = _sdk.PromoCodes.GetHistoryAsync();
            operation.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[PromoCodes] GetHistory failed: {completed.Result}");
                    return;
                }

                foreach (var item in completed.Result.Data)
                {
                    Debug.Log($"[PromoCodes] History: {item.campaignKey} at {item.redeemedAt} " +
                              $"(expires {item.effectExpiresAt ?? "—"})");
                }
            };
        }

        public void GetActiveEffects()
        {
            var operation = _sdk.PromoCodes.GetActiveEffectsAsync();
            operation.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError($"[PromoCodes] GetActiveEffects failed: {completed.Result}");
                    return;
                }

                foreach (var effect in completed.Result.Data)
                {
                    Debug.Log($"[PromoCodes] Active effect '{effect.key}' from campaign {effect.campaignId}, " +
                              $"expires {effect.expiresAt ?? "permanent"}.");
                }
            };
        }
    }
}
