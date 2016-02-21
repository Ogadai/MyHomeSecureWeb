using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyHomeSecureWeb.Repositories
{
    public class AwayStatusRepository : IAwayStatusRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        public AwayStatus GetStatus(string userName)
        {
            return db.AwayStatus.SingleOrDefault(s => s.UserName == userName);
        }

        public AwayStatus GetStatusFromGoogleToken(string token)
        {
            return db.AwayStatus.SingleOrDefault(s => s.GoogleToken == token);
        }

        public void UpdateStatus(string userName, bool away)
        {
            db.AwayStatus.Single(s => s.UserName == userName).Away = away;
            db.SaveChanges();
        }

        public void SetGoogleToken(string userName, string token)
        {
            db.AwayStatus.Single(s => s.UserName == userName).GoogleToken = token;
            db.SaveChanges();
        }

        public void SetToken(string userName, byte[] tokenHash)
        {
            db.AwayStatus.Single(s => s.UserName == userName).TokenHash = tokenHash;
            db.SaveChanges();
        }

        public void AddUser(string userName, string homeHubId, byte[] tokenHash, byte[] salt)
        {
            if (db.AwayStatus.SingleOrDefault(s => s.UserName == userName) != null)
            {
                throw new Exception(string.Format("The user '{0}' already exists", userName));
            }

            db.AwayStatus.Add(new AwayStatus
            {
                Id = Guid.NewGuid().ToString(),
                HomeHubId = homeHubId,
                UserName = userName,
                Away = false,
                TokenHash = tokenHash,
                TokenSalt = salt
            });
            db.SaveChanges();
        }

        public void RemoveUser(string userName)
        {
            db.AwayStatus.Remove(db.AwayStatus.Single(s => s.UserName == userName));
            db.SaveChanges();
        }

        public IQueryable<AwayStatus> GetAllForHub(string homeHubId)
        {
            return db.AwayStatus.Where(s => s.HomeHubId == homeHubId);
        }
        
        public void Dispose()
        {
            db.Dispose();
        }

    }
}
