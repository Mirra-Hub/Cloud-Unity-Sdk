using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class LoginAsGuestDto
    {
        public string GuestId;
        public bool CreateAccount;
        /// <summary>
        /// Base nickname provided by the client. Required when CreateAccount=true and the
        /// account does not yet exist; ignored otherwise. The server validates regex and
        /// uniqueness and appends a random suffix according to project settings.
        /// </summary>
        public string Nickname;
    }
}

