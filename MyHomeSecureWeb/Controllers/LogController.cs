using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LogController : ApiController
    {
        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private ILogRepository _logRepository = new LogRepository();
        private IPasswordHash _passwordHash = new PasswordHash();

        // GET api/log/id
        public IHttpActionResult GetLogEntries(string id, string token)
        {
            var hub = _homeHubRepository.GetHub(id);
            if (hub == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            var tokenHash = _passwordHash.Hash(token, hub.TokenSalt);
            if (!tokenHash.SequenceEqual(hub.TokenHash))
            {
                return Unauthorized();
            }

            var entries = _logRepository.GetLogEntries(hub.Id)
                    .Select(l => new LogEntryResponse { Message = l.Message, Time = l.Time });

            return Ok(entries);
        }
    }
}
