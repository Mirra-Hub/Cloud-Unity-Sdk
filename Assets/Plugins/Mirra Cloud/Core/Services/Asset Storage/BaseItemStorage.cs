using System;

namespace MirraCloud.Core.AssetsStorage
{
    public abstract class BaseItemStorage
    {
        public readonly string ItemId;
        public readonly string Name;
        public readonly string Path;
        public readonly DateTime CreatedAt;
        public readonly DateTime UpdatedAt;

        protected BaseItemStorage(BaseItemStorageDto dto)
        {
            ItemId = dto.itemId;
            Name = dto.name;
            CreatedAt = dto.createdAt;
            UpdatedAt = dto.updatedAt;
            Path = dto.path;
        }
    }
}