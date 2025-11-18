using System;
using System.Collections.Generic;
using MirraCloud.Core.Tournaments.Enums;

namespace Plugins.MirraCloud.Core.Services.Tournaments.Dto
{
    [Serializable]
    public class TournamentConfigDto
    {
        public string id;
        public string name;

        public TournamentTableDto[] tables;

        public OrderType orderType;
        public TournamentsType type;
        public UpdateStrategy updateStrategy;

        public ResetIntervalType resetIntervalType;
        public DateTime? nextResetDate;
        public DateTime? lastResetDate;

        public DateTime createdDate;
        public DateTime updatedDate;
    }
}