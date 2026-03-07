using System.Collections.Generic;

namespace MirraCloud.Core.CloudSave.Responses
{
    public class FileItemResponse
    {
        public string key;
        public Dictionary<string, string> meta;
        public PrincipalMask readMask;
        public PrincipalMask writeMask;
        public long fileSize;
        public string extension;
        public string mimeType;
        public string createdAtUtc;
        public string updatedAtUtc;
    }
}
