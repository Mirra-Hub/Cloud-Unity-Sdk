using System;

namespace MirraCloud.Core.Leaderboard.Dto
{
    [Serializable]
    public class LeaderboardTopAndPlayersAroundDto
    {
        public string leaderboardId;

        public LeaderboardEntryDto[] tTop;
        public LeaderboardEntryDto[] playersAround;
    }
}