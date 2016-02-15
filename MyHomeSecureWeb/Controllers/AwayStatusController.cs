using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Utilities;
using System.Web.Http.Cors;
using System;
using MyHomeSecureWeb.Repositories;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class AwayStatusController : ApiController
    {
        private IAwayStatusRepository _awayStatusRepository = new AwayStatusRepository();
        private ILogRepository _logRepository = new LogRepository();
        private IPasswordHash _passwordHash = new PasswordHash();
        private ILookupToken _lookupToken = new LookupToken();

        private static string ActionEntered = "entered";
        private static string ActionExited = "exited";

        // POST: api/AwayStatus
        [ResponseType(typeof(AwayStatus))]
        public async Task<IHttpActionResult> PostAwayStatus(AwayStatusRequest awayStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(awayStatus.UserName) && string.IsNullOrEmpty(awayStatus.Token))
            {
                var emailAddress = await _lookupToken.GetEmailAddress(this.User);
                if (string.IsNullOrEmpty(emailAddress))
                {
                    return Unauthorized();
                }

                var existingEntry = _awayStatusRepository.GetStatus(awayStatus.UserName);
                if (existingEntry == null)
                {
                    return NotFound();
                }

                UpdateAwayStatus(existingEntry, awayStatus.Action);
            }
            else
            {

                var existingEntry = _awayStatusRepository.GetStatus(awayStatus.UserName);
                if (existingEntry == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(awayStatus.Token))
                {
                    return Unauthorized();
                }

                var tokenHash = _passwordHash.Hash(awayStatus.Token, existingEntry.TokenSalt);
                if (!tokenHash.SequenceEqual(existingEntry.TokenHash))
                {
                    return Unauthorized();
                }

                UpdateAwayStatus(existingEntry, awayStatus.Action);
            }


            return StatusCode(HttpStatusCode.NoContent);
        }

        private void UpdateAwayStatus(AwayStatus existingEntry, string awayStatusAction)
        {
            var newAwayStatus = string.Equals(awayStatusAction, ActionExited, StringComparison.OrdinalIgnoreCase);
            if (newAwayStatus != existingEntry.Away)
            {
                _awayStatusRepository.UpdateStatus(existingEntry.UserName, newAwayStatus);
                CheckInOutMonitor.UserInOut(existingEntry.HomeHubId, existingEntry.UserName, newAwayStatus);
                _logRepository.Priority(existingEntry.HomeHubId,
                            string.Format("{0} {1}", existingEntry.UserName, newAwayStatus ? ActionExited : ActionEntered));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _awayStatusRepository.Dispose();
                _logRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}