using System;
using UnityEngine;

namespace MirraCloud.Core.Auth
{
    public class PlayerAccountInfo
    {
        public string Name { get; private set; }
        public uint Age { get; private set; }
        public string PlayerId { get; private set; }
        
        public DateTime CreateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        
        public PlayerAccountInfo(PlayerAccountInfoDto dto)
        {
            PlayerId = dto.playerId;
            CreateDate = dto.createDate;
            LastLoginDate = dto.lastLoginDate;
            Name = dto.username;
            Age = dto.age;
        }
    }
}