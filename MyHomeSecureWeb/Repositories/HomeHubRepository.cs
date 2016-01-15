using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
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

        public HomeHub AddHub(string name, byte[] tokenHash, byte[] salt)
        {
            var hub = new HomeHub
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                TokenHash = tokenHash,
                TokenSalt = salt
            };

            var newHub = db.HomeHubs.Add(hub);
            db.SaveChanges();

            return newHub;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
