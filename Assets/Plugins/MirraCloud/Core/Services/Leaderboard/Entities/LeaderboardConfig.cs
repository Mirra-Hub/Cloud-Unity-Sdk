using System;
using MirraCloud.Core.Leaderboard.Dto;
using MirraCloud.Core.Leaderboard.Enums;
using OrderType = Codice.CM.Common.OrderType;

namespace MirraCloud.Core.Leaderboard.Entities
{
    public class LeaderboardConfig
    {
        public readonly string Id;
        public readonly string Name;

        public readonly RewardRangeDto[] RewardsForPlaces;
    
        public readonly OrderType OrderType ;
        public readonly LeaderboardType Type;
        public readonly UpdateStrategy UpdateStrategy;

        public readonly bool IsReset;
        
        public readonly ResetIntervalType ResetIntervalType;
        public readonly DateTime? NextResetDate;
        public readonly DateTime? LastResetDate;

        public readonly DateTime CreatedDate;
        public readonly DateTime UpdatedDate;
        
        public LeaderboardConfig(LeaderboardConfigDto dto)
        {
            Id = dto.id;
            Name = dto.name;
            RewardsForPlaces = dto.rewardsForPlaces;
            OrderType = dto.orderType;
            Type = dto.type;
            UpdateStrategy = dto.updateStrategy;
            IsReset = dto.isReset;
            ResetIntervalType = dto.resetIntervalType;
            NextResetDate = dto.nextResetDate;
            LastResetDate = dto.lastResetDate;
            CreatedDate = dto.createdDate;
            UpdatedDate = dto.updatedDate;
        }
    }
}