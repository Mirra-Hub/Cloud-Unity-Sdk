using System;

namespace MirraCloud
{
    [Serializable]
    public struct ChangeCurrencyOperationDto
    {
        public string CurrencyId;
        public int Amount;
    }
}