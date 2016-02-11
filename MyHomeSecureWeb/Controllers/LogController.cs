using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class LogController : ApiController
    {
        private IHomeHubRepository _homeHubRepository = new HomeHubRepository();
        private ILogRepository _logRepository = new LogRepository();
        private IPasswordHash _passwordHash = new PasswordHash();
        private ILookupToken _lookupToken = new LookupToken();

        // GET api/log
        [AuthorizeLevel(AuthorizationLevel.User)]
        public async Task<IHttpActionResult> GetLog()
        {
            var hubId = await _lookupToken.GetHomeHubId(this.User);
            return GetLogEntriesForHub(hubId);
        }

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

            return GetLogEntriesForHub(hub.Id);
        }

        private IHttpActionResult GetLogEntriesForHub(string hubId)
        {
            return Ok(_logRepository.GetLogEntries(hubId)
                    .Select(l => new LogEntryResponse { Message = l.Message, Time = l.Time }));
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _homeHubRepository.Dispose();
                _logRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
