using System;

namespace MirraCloud.Core.Leaderboard
{
    [Serializable]
    public struct LeaderboardValue
    {
        public string PlayerId;
        public int Score;
        public int Position;
    }
}