using System;

namespace MirraCloud.Core.PromoCodes.Dto
{
    [Serializable]
    public sealed class GrantedRewardDto
    {
        public string rewardId;
        public int economyResourceKind;
        public int count;
    }
}
