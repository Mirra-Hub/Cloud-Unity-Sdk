namespace MirraCloud.Core.AssetsStorage
{
    public class Asset : BaseItemStorage
    {
        public readonly string FolderId;
        public readonly string MimeType;
        public readonly long Size;
        public readonly int Version;
        public readonly AssetType Type;
        public readonly string Extension;

        public Asset(AssetDto dto) : base(dto)
        {
            FolderId = dto.folderId;
            MimeType = dto.mimeType;
            Size = dto.size; 
            Version = dto.version;
            Type = dto.type; 
            Extension = dto.extension;
        }
    }
}