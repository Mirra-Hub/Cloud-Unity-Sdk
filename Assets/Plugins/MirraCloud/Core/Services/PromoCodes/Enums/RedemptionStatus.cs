namespace MirraCloud.Core.PromoCodes.Enums
{
    /// <summary>
    /// Mirror of backend <c>PromoCodes.Enums.RedemptionStatus</c>. Use the integer values
    /// directly when comparing against <see cref="Dto.RedeemPromoCodeResponseDto.status"/>.
    /// </summary>
    public static class RedemptionStatus
    {
        public const int Success = 1;
        public const int InvalidCode = 2;
        public const int Expired = 3;
        public const int NotYetActive = 4;
        public const int Disabled = 5;
        public const int LimitExceeded = 6;
        public const int RuleFailed = 7;
        public const int AlreadyRedeemed = 8;
        public const int CodeBlocked = 9;
    }
}
