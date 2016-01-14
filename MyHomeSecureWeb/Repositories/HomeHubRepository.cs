using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public class HomeHubRepository : IHomeHubRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        public HomeHub GetHub(string name)
        {
            return db.HomeHubs.SingleOrDefault(h => h.Name == name);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
