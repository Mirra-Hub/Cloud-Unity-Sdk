using System;
using System.Collections.Generic;

namespace MirraCloud.Core.PromoCodes.Dto
{
    [Serializable]
    public sealed class RedeemPromoCodeResponseDto
    {
        /// <summary>Mirror of backend <c>RedemptionStatus</c> enum. See <see cref="Enums.RedemptionStatus"/>.</summary>
        public int status;

        public string campaignId;
        public string campaignKey;
        public string campaignDisplayName;

        public List<GrantedRewardDto> rewards = new List<GrantedRewardDto>();
        public List<GrantedEffectDto> effects = new List<GrantedEffectDto>();
    }
}
