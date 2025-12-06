using System;

namespace MirraCloud.Core.Auth
{
    /// <summary>
    /// DTO для резолва конфликта привязки провайдера.
    /// </summary>
    [Serializable]
    public class LinkAuthProviderDto
    {
        public int ProviderType;
        public string TargetAccountId;
    }
}

