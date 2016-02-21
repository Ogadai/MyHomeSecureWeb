using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHomeSecureWeb.Utilities
{
    public class LookupToken : ILookupToken
    {
        private const string CacheKeyTemplate = "GoogleTokenUser_{0}";
        private const string GoogleTokenUrl = "https://www.googleapis.com/oauth2/v1/userinfo?access_token={0}";

        public async Task<string> GetEmailAddress(IPrincipal user)
        {
            var identities = await(user as ServiceUser).GetIdentitiesAsync();

            //Check if the user has logged in using Google as Identity provider
            var google = identities.OfType<GoogleCredentials>().FirstOrDefault();
            if (google != null)
            {
                var cachedEmail = GetEmailFromCache(google.AccessToken);
                if (!string.IsNullOrEmpty(cachedEmail))
                {
                    return cachedEmail;
                }

                var googleInfo = await GetProviderInfo(google.AccessToken);
                var userEmail = googleInfo.Value<string>("email");

                SetEmailInCache(google.AccessToken, userEmail);

                return userEmail;
            }
            return null;
        }

        public async Task<string> GetHomeHubId(IPrincipal user)
        {
            var emailAddress = await GetEmailAddress(user);
            if (emailAddress != null)
            {
                using (var awayStatusRepository = new AwayStatusRepository())
                {
                    var awayStatus = awayStatusRepository.GetStatus(emailAddress);
                    if (awayStatus != null)
                    {
                        return awayStatus.HomeHubId;
                    }
                }
            }
            return null;
        }

        private void SetUserGoogleToken(string emailAddress, string token)
        {
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var existingUser = awayStatusRepository.GetStatus(emailAddress);
                if (existingUser != null)
                {
                    awayStatusRepository.SetGoogleToken(emailAddress, token);
                }
            }
        }

        private string GetEmailFromCache(string accessToken)
        {
            var cache = HttpContext.Current != null ? HttpContext.Current.Cache : null;
            return cache != null ? cache[CacheKey(accessToken)] as string : null;
        }
        private void SetEmailInCache(string accessToken, string emailAddress)
        {
            var cache = HttpContext.Current != null ? HttpContext.Current.Cache : null;
            if (cache != null)
            {
                cache[CacheKey(accessToken)] = emailAddress;
            }
        }
        private string CacheKey(string accessToken)
        {
            return string.Format(CacheKeyTemplate, accessToken);
        }

        private async Task<JToken> GetProviderInfo(string accessToken)
        {
            string url = string.Format(GoogleTokenUrl, accessToken);
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return JToken.Parse(await response.Content.ReadAsStringAsync());
            }
        }
    }


}
