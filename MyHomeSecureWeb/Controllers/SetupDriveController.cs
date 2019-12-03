using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Utilities;
using System.Threading.Tasks;
using System.Web.Http;
using MyHomeSecureWeb.Repositories;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [GoogleAuthorisation(AuthorizationLevel.Anonymous)]
    [RequireHttps]
    public class SetupDriveController : GoogleOath2Controller
    {
        protected override string TargetAction => "SetupDriveCode";
        protected override string CacheStateKey => "setupdrive-state-index";

        [HttpGet]
        [Route("api/setupdrive", Name = "SetupDrive")]
        public IHttpActionResult SetupDrive()
        {
            return BeginAuth();
        }

        [HttpGet]
        [Route("api/setupdrive/code", Name = "SetupDriveCode")]
        public async Task<IHttpActionResult> SetupDriveCode(string state = null, string code = null, string error = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Services.Log.Error(string.Format("Error requesting Google Drive code: {0}", error));
                return InternalServerError();
            }

            if (!VerifyStateString(state))
            {
                Services.Log.Error(string.Format("Invalid state token from Google Drive authorisation request: {0}", state));
                return Unauthorized();
            }

            var token = await GetAccessToken(code);

            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                await RevokeToken(token.AccessToken);
                return SetupDrive();
            }
            else
            {
                var emailAddress = await GetUserEmail(token.AccessToken);

                if (CheckUserExists(emailAddress))
                {
                    // Store the tokens
                    await StoreDriveTokens(emailAddress, token.AccessToken, token.RefreshToken);

                    return new HtmlActionResult("SetupDriveSuccess", new { UserName = emailAddress });
                }
                else
                {
                    return new HtmlActionResult("SetupDriveNotFound", new { UserName = emailAddress });
                }
            }
        }

        private async Task StoreDriveTokens(string emailAddress, string accessToken, string refreshToken)
        {
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                await awayStatusRepository.SetDriveTokensAsync(emailAddress, accessToken, refreshToken);
            }
        }
    }
}
