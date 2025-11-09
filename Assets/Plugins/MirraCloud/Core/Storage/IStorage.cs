namespace MirraCloud.Core.Storage
{
    public interface IStorage
    {
        bool HasKey(string guestIDKey);
        string GetString(string guestIDKey);
        void SaveString(string guestIDKey, string guestId);
    }
}