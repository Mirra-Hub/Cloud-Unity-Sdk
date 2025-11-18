using System;
using MirraCloud.Core.Tournaments.Enums;
using Plugins.MirraCloud.Core.Services.Tournaments.Dto;

namespace Plugins.MirraCloud.Core.Services.Tournaments
{
    [Serializable]
    public class TournamentConfig
    {
        public readonly string Id;
        public readonly string Name;
        
        public readonly TournamentTableDto[] tables;
        
        public readonly OrderType OrderType ;
        public readonly TournamentsType Type;
        public readonly UpdateStrategy UpdateStrategy;
        
        public readonly ResetIntervalType ResetIntervalType;
        public readonly DateTime? NextResetDate;
        public readonly DateTime? LastResetDate;

        public readonly DateTime CreatedDate;
        public readonly DateTime UpdatedDate;

        public TournamentConfig(TournamentConfigDto dto)
        {
            Id = dto.id;
            Name = dto.name;
            OrderType = dto.orderType;
            Type = dto.type;
            UpdateStrategy = dto.updateStrategy;
            ResetIntervalType = dto.resetIntervalType;
            NextResetDate = dto.nextResetDate;
            LastResetDate = dto.lastResetDate;
            CreatedDate = dto.createdDate;
            UpdatedDate = dto.updatedDate;

            tables = dto.tables;
        }
    }
}