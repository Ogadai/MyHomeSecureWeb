using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Utilities;
using System.Threading.Tasks;
using System.Web.Http;
using MyHomeSecureWeb.Repositories;
using System.Net;
using MyHomeSecureWeb.Models;

namespace MyHomeSecureWeb.Controllers
{
    public class PushStateRequest
    {
        public string Token { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }

    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.Anonymous)]
    [RequireHttps]
    public class PushStateController : GoogleOath2Controller
    {
        protected override string TargetAction => "PushStateCode";
        protected override string CacheStateKey => "pushstate-index";

        [HttpGet]
        [Route("api/pushstate", Name = "SetupPushState")]
        public IHttpActionResult SetupDrive()
        {
            return BeginAuth();
        }

        [HttpPost]
        [Route("api/pushstate", Name = "PushState")]
        public IHttpActionResult PushState(PushStateRequest request)
        {
            string homeHubId = null;
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var userAwayStatus = awayStatusRepository.LookupMakerToken(request.Token);
                if (userAwayStatus == null)
                {
                    Services.Log.Error("User not found", null, "PushState");
                    return NotFound();
                }

                homeHubId = userAwayStatus.HomeHubId;
            }

            // Get the chatHub for this HomeHub
            var chatHub = ChatHub.Get(homeHubId);

            // Send it a message
            var messageStates = new HubChangeStates
            {
                States = new[] {
                    new HubChangeState
                    {
                        Name = request.Name,
                        Active = request.Active
                    }
                }
            };
            chatHub.MessageToHome(messageStates);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [Route("api/pushstate/code", Name = "PushStateCode")]
        public async Task<IHttpActionResult> PushStateCode(string state = null, string code = null, string error = null, bool forceRenew = false)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Services.Log.Error(string.Format("Error requesting Google authorisation code: {0}", error));
                return InternalServerError();
            }

            if (!VerifyStateString(state))
            {
                Services.Log.Error(string.Format("Invalid state token from Google authorisation request: {0}", state));
                return Unauthorized();
            }

            var token = await GetAccessToken(code);

            var emailAddress = await GetUserEmail(token.AccessToken);

            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var userAwayStatus = awayStatusRepository.GetStatus(emailAddress);
                if (userAwayStatus != null)
                {
                    var makerToken = userAwayStatus.MakerApiToken;
                    if (forceRenew || string.IsNullOrEmpty(makerToken))
                    {
                        makerToken = _passwordHash.CreateToken(128);
                        awayStatusRepository.SetMakerToken(emailAddress, makerToken);
                    }

                    return new HtmlActionResult("PushStateViewLink", new {
                        UserName = emailAddress,
                        Token = makerToken,
                        Link = Url.Link("PushState", null)
                    });
                }
                else
                {
                    return new HtmlActionResult("PushStateNotFound", new { UserName = emailAddress });
                }
            }
        }
    }
}
