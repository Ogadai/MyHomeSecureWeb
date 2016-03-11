using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Utilities;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using Newtonsoft.Json;
using MyHomeSecureWeb.Models;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [RequireHttps]
    public class SetupDriveController : ApiController
    {
        public ApiServices Services { get; set; }

        private IPasswordHash _passwordHash = new PasswordHash();

        private const string _authUrlTemplate = "https://accounts.google.com/o/oauth2/v2/auth?access_type=offline&approval_prompt=force&scope={0}&state={1}&redirect_uri={2}&response_type=code&client_id={3}&i={4}";
        private const string _tokenExchangeAddress = "https://www.googleapis.com/oauth2/v4/token";
        private const string _revokeAddressTemplate = "https://accounts.google.com/o/oauth2/revoke?token={0}";
        private const string _lookupEmailAddress = "https://www.googleapis.com/oauth2/v2/userinfo";

        private const string _tokenExchangeBodyTemplate = "grant_type=authorization_code&code={0}&redirect_uri={1}&client_id={2}&client_secret={3}";
        private const string _scope = "https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fdrive.file%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email";

        [HttpGet]
        [Route("api/setupdrive", Name = "SetupDrive")]
        public IHttpActionResult SetupDrive()
        {
            var clientId = WebConfigurationManager.AppSettings["ClientId"];

            var redirect = string.Format(_authUrlTemplate, _scope, GetStateString(), GetReturnUrlEnq(), clientId, _randGet.Next(1000));
            return Redirect(redirect);
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

                return Ok(emailAddress);
            }
        }

        private async Task<GoogleAccessToken> GetAccessToken(string code)
        {
            var clientId = WebConfigurationManager.AppSettings["ClientId"];
            var clientSecret = WebConfigurationManager.AppSettings["ClientSecret"];

            var returnUrl = Url.Link("SetupDriveCode", new { });
            var postData = string.Format(_tokenExchangeBodyTemplate, code, GetReturnUrlEnq(), clientId, clientSecret);
            var byteArray = Encoding.UTF8.GetBytes(postData);

            // Get the response
            var request = WebRequest.Create(_tokenExchangeAddress);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            request.Method = "POST";

            using (var writeStream = await request.GetRequestStreamAsync())
            {
                writeStream.Write(byteArray, 0, byteArray.Length);
            }

            var response = await request.GetResponseAsync();

            string responseContent = null;
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
            }

            // Deserialise for the access token and renewal token
            return JsonConvert.DeserializeObject<GoogleAccessToken>(responseContent);
        }

        private async Task RevokeToken(string accessToken)
        {
            var url = string.Format(_revokeAddressTemplate, accessToken);
            var request = WebRequest.Create(url);

            await request.GetResponseAsync();
        }

        private async Task<string> GetUserEmail(string accessToken)
        {
            var request = WebRequest.Create(_lookupEmailAddress); //  + "?access_token=" + accessToken)
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", accessToken);

            var response = await request.GetResponseAsync();
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    var userInfoJson = await reader.ReadToEndAsync();
                    var userInfo = JsonConvert.DeserializeObject<GoogleUserInfo>(userInfoJson);
                    return userInfo.Email;
                }
            }

        }

        private string GetReturnUrlEnq()
        {
            return HttpUtility.UrlEncode(Url.Link("SetupDriveCode", new { }));
        }

        private Random _randGet = new Random();
        private string GetStateString()
        {
            var state = _passwordHash.CreateToken(128);

            var index = _randGet.Next(10000);
            while(HttpContext.Current.Cache[CacheStateIndex(index)] != null)
            {
                index = _randGet.Next(10000);
            }
            HttpContext.Current.Cache[CacheStateIndex(index)] = state;

            return string.Format("{0}|{1}", index, state);
        }

        private bool VerifyStateString(string submittedState)
        {
            var separator = submittedState.IndexOf('|');
            if (separator != -1)
            {
                var index = int.Parse(submittedState.Substring(0, separator));
                var state = submittedState.Substring(separator + 1);

                var storedState = HttpContext.Current.Cache[CacheStateIndex(index)] as string;
                if (storedState != null && string.Equals(storedState, state))
                {
                    HttpContext.Current.Cache.Remove(CacheStateIndex(index));
                    return true;
                }
            }

            return false;
        }

        private string CacheStateIndex(int index)
        {
            return string.Format("setupdrive-state-index-{0}", index);
        }
    }
}
