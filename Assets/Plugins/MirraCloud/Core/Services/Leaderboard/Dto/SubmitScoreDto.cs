using System;

namespace MirraCloud.Core.Leaderboard.Dto
{
    [Serializable]
    public sealed record SubmitScoreDto
    {
        public string PlayerName;
        public double Value;
    }
}