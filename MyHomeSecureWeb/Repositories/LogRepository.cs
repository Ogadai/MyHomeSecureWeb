﻿using MyHomeSecureWeb.DataObjects;
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
            LogEntry(homeHubId, "Info", message);
        }

        public void Error(string homeHubId, string message)
        {
            LogEntry(homeHubId, "Error", message);
        }

        private void LogEntry(string homeHubId, string severity, string message)
        {
            db.LogEntries.Add(new LogEntry
            {
                Id = Guid.NewGuid().ToString(),
                Severity = severity,
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

        public void PurgeOldLogEntries()
        {
            db.Database.ExecuteSqlCommand(
                "DELETE FROM MyHomeSecureWeb.LogEntries WHERE Time < {0}",
                DateTime.Now.AddDays(-2));
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
