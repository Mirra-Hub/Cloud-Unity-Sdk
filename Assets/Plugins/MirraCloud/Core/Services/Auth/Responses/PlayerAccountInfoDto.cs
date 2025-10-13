using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class PlayerAccountInfoDto
    {
        public string username;
        public uint age;
        public string avatarUrl;

        public string playerId;
        public DateTime createDate;
        public DateTime lastLoginDate;

        /*public Language Language { get; init; }
        public Country Country { get; init; }*/
    }
}