using MirraCloud.Json;

namespace MirraCloud.Core.CloudSave.Responses
{
    public class DataItemResponse
    {
        public string key;
        public JsonValue value;
        public CloudSaveFieldType fieldType;
        public PrincipalMask readMask;
        public PrincipalMask writeMask;
        public string updatedAtUtc;
        public long version;
    }
}
