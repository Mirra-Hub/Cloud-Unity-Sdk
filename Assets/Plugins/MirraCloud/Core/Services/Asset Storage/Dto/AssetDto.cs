using System;

namespace MirraCloud.Core.AssetsStorage
{
    [Serializable]
    public class AssetDto : BaseItemStorageDto
    {
        public string folderId;
        public string mimeType;
        public long size;
        public int version;
        public AssetType type;
        public string extension;
    }
}