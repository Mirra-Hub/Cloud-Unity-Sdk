using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class GetAuthDataDto
    {
        public AuthResultStatus Status;
        public string Token;
        public AccountDto PlayerInfo;
        public AccountDto CurrentAccount;
        public AccountDto ExistingAccount;
        public string GuestId;
        public SessionInfoDto Session;
    }
}

