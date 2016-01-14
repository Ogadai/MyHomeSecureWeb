namespace MyHomeSecureWeb.Utilities
{
    public interface IPasswordHash
    {
        byte[] CreateSalt(int size);
        byte[] Hash(string value, byte[] salt);
        byte[] Hash(byte[] value, byte[] salt);
    }
}