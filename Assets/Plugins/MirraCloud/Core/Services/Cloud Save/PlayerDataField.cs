namespace MirraCloud.Core.CloudSave
{
    public struct PlayerDataField
    {
        public string Key;
        public string CurrentValue;
        public CloudSaveFieldType FieldType;
        public PrincipalMask ReadMask;
        public PrincipalMask WriteMask;
        public long Version;
        public string UpdatedAtUtc;

        public PlayerDataField(string key, string currentValue, CloudSaveFieldType fieldType,
            PrincipalMask readMask = PrincipalMask.None, PrincipalMask writeMask = PrincipalMask.None,
            long version = 0, string updatedAtUtc = null)
        {
            Key = key;
            CurrentValue = currentValue;
            FieldType = fieldType;
            ReadMask = readMask;
            WriteMask = writeMask;
            Version = version;
            UpdatedAtUtc = updatedAtUtc;
        }
    }
}
