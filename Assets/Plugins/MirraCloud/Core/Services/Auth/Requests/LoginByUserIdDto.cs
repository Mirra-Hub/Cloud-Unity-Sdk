using System;

namespace MirraCloud.Core.Auth
{
    /// <summary>
    /// Унифицированный DTO для провайдеров, где достаточно userId (vk, yandex-games, mobile и т.п.).
    /// </summary>
    [Serializable]
    public class LoginByUserIdDto
    {
        public string UserId;
        public bool CreateAccount;
    }
}

