using System;

namespace MirraCloud.Core.AssetsStorage
{
    [Serializable]
    public abstract class BaseItemStorageDto
    {
        public string itemId;
        public string name;
        public string path;

        public DateTime createdAt;
        public DateTime updatedAt;
    }
}