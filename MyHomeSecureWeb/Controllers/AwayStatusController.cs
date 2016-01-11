using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
using System.Data.Entity.Validation;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AwayStatusController : ApiController
    {
        private MobileServiceContext db = new MobileServiceContext();
        private PasswordHash _passwordHash = new PasswordHash();

        // GET: api/AwayStatus
        public IQueryable<AwayStatusResponse> GetAwayStatus()
        {
            return db.AwayStatus.Select(s => new AwayStatusResponse { UserName = s.UserName, Away = s.Away });
        }
        
        // POST: api/AwayStatus
        [ResponseType(typeof(AwayStatus))]
        public IHttpActionResult PostAwayStatus(AwayStatusRequest awayStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEntry = db.AwayStatus.SingleOrDefault(s => s.UserName == awayStatus.UserName);
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

            existingEntry.Away = awayStatus.Away;

            db.LogEntries.Add(new LogEntry {
                Id = Guid.NewGuid().ToString(),
                HomeHubId = existingEntry.HomeHubId,
                Message = string.Format("{0} {1}", existingEntry.UserName, awayStatus.Away ? "exited" : "entered"),
                Time = DateTime.Now
            });

            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
                
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}