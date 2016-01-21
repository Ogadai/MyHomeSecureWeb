using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using MyHomeSecureWeb.Repositories;

namespace MyHomeSecureWeb.ScheduledJobs
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/purgelog".
    public class PurgeLogJob : ScheduledJob
    {

        public override Task ExecuteAsync()
        {
            Services.Log.Info("Hello from scheduled job!");
            using (var logRepository = new LogRepository())
            {
                logRepository.PurgeOldLogEntries();
            }

            return Task.FromResult(true);
        }
    }
}