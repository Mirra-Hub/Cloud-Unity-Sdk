using System;

namespace MirraCloud.Core.Auth
{
    public class PlayerAccountInfo
    {
        public string Name { get; private set; }
        public uint Age { get; private set; }
        
        public PlayerAccountInfo(PlayerAccountInfoResponse playerInfoResponse)
        {
            Name = playerInfoResponse.username;
            Age = playerInfoResponse.age;
        }
    }
}