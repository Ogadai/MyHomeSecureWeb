using MyHomeSecureWeb.DataObjects;

namespace MyHomeSecureWeb.Repositories
{
    public interface IHomeHubRepository
    {
        void Dispose();
        HomeHub GetHub(string name);
        HomeHub AddHub(string name, byte[] tokenHash, byte[] salt);
    }
}