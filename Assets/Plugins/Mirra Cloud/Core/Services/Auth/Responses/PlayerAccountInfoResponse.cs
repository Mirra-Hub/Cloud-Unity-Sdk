using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class PlayerAccountInfoResponse
    {
        public string username;
        public uint age;
        public string avatarUrl;
        
        public string playerId { get; set; }
        public DateTime createDate { get; set; }
        public DateTime lastLoginDate { get; set; }
  
        /*public Language Language { get; init; }
        public Country Country { get; init; }*/
    }
}