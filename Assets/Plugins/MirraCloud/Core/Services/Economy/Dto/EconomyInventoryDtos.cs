using System;
using System.Collections.Generic;
using MirraCloud.Json;

namespace MirraCloud.Core.Economy.Dto
{
    [Serializable]
    public sealed class PlayerInventoryDto
    {
        [JsonNameCamel] public Dictionary<string, decimal> Resources;
    }

    [Serializable]
    public sealed class RewardDto
    {
        [JsonNameCamel] public string Key;
        [JsonNameCamel] public decimal Amount;
    }

    [Serializable]
    public sealed class ModifyResourceAmountDto
    {
        [JsonNameCamel] public string Key;
        [JsonNameCamel] public decimal Amount;
    }

    [Serializable]
    public sealed class GrantRewardsDto
    {
        [JsonNameCamel] public RewardDto[] Rewards;
    }

    [Serializable]
    public sealed class ConsumeContainerDto
    {
        [JsonNameCamel] public string Key;
    }

    [Serializable]
    public sealed class OpenLootboxDto
    {
        [JsonNameCamel] public string Key;
    }

    [Serializable]
    public sealed class InventoryWithRewardsDto
    {
        [JsonNameCamel] public PlayerInventoryDto Inventory;
        [JsonNameCamel] public RewardDto[] Rewards;
    }
}
