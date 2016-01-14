using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public class LogRepository : ILogRepository
    {
        private MobileServiceContext db = new MobileServiceContext();

        public void Info(string homeHubId, string message)
        {
            db.LogEntries.Add(new LogEntry
            {
                Id = Guid.NewGuid().ToString(),
                HomeHubId = homeHubId,
                Message = message,
                Time = DateTime.Now
            });

            db.SaveChanges();
        }

        public IQueryable<LogEntry> GetLogEntries(string homeHubId)
        {
            return db.LogEntries.Where(l => string.Equals(l.HomeHubId, homeHubId));
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
