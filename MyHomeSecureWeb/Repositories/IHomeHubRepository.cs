using MyHomeSecureWeb.DataObjects;

namespace MyHomeSecureWeb.Repositories
{
    public interface IHomeHubRepository
    {
        void Dispose();
        HomeHub GetHub(string name);
        HomeHub GetHubById(string id);
        HomeHub AddHub(string name, byte[] tokenHash, byte[] salt);
        void SetLocation(string homeHubId, double latitude, double longitude, float radius);
    }
}