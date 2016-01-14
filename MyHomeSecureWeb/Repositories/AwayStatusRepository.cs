using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public class AwayStatusRepository : IAwayStatusRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        public AwayStatus GetStatus(string userName)
        {
            return db.AwayStatus.SingleOrDefault(s => s.UserName == userName);
        }

        public void UpdateStatus(string userName, bool away)
        {
            db.AwayStatus.Single(s => s.UserName == userName).Away = away;
            db.SaveChanges();
        }

        public void Dispose()
        {
            db.Dispose();
        }

    }
}
