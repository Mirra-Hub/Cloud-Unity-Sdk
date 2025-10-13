using System;

namespace MirraCloud.Core.CloudSave.Responses
{
    [Serializable]
    public class PlayerDataResponse
    {
        public string projectId;
        public PlayerDataField[] data;
        
        [Serializable]
        public struct PlayerDataField
        {
            public string key;
            public string value;
            public CloudSaveFieldType fieldType;
        }
    }
}