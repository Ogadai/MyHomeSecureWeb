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

        private static string ActionEntered = "entered";
        private static string ActionExited = "exited";

        // POST: api/AwayStatus
        [ResponseType(typeof(AwayStatus))]
        public IHttpActionResult PostAwayStatus(AwayStatusRequest awayStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            var newAwayStatus = string.Equals(awayStatus.Action, ActionExited, StringComparison.OrdinalIgnoreCase);
            if (newAwayStatus != existingEntry.Away)
            {
                _awayStatusRepository.UpdateStatus(awayStatus.UserName, newAwayStatus);
                CheckInOutMonitor.UserInOut(existingEntry.HomeHubId, awayStatus.UserName, newAwayStatus);
                _logRepository.Info(existingEntry.HomeHubId,
                            string.Format("{0} {1}", awayStatus.UserName, newAwayStatus ? ActionExited : ActionEntered));
            }

            return StatusCode(HttpStatusCode.NoContent);
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