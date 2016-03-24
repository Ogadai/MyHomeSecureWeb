using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Repositories;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.Utilities
{
    public class LookupToken : ILookupToken
    {
        private const string GoogleTokenUrl = "https://www.googleapis.com/oauth2/v1/userinfo?access_token={0}";

        public async Task<string> GetEmailAddress(IPrincipal user)
        {
            try
            {
                var identities = await (user as ServiceUser).GetIdentitiesAsync();

                //Check if the user has logged in using Google as Identity provider
                var google = identities.OfType<GoogleCredentials>().FirstOrDefault();
                if (google != null)
                {
                    var cachedEmail = LookupEmailFromToken(google.AccessToken);
                    if (!string.IsNullOrEmpty(cachedEmail))
                    {
                        return cachedEmail;
                    }

                    var googleInfo = await GetProviderInfo(google.AccessToken);
                    var userEmail = googleInfo.Value<string>("email");

                    await StoreToken(google.AccessToken, userEmail);

                    return userEmail;
                }
            }
            catch(HttpRequestException requestException)
            {
                // Swallow error and return null
            }
            return null;
        }

        public async Task<string> GetHomeHubId(IPrincipal user)
        {
            var emailAddress = await GetEmailAddress(user);

            return GetHomeHubIdFromEmail(emailAddress);
        }

        public string GetHomeHubIdFromEmail(string emailAddress)
        {
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

        private string LookupEmailFromToken(string accessToken)
        {
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                var awayStatus = awayStatusRepository.LookupGoogleToken(accessToken);
                return awayStatus != null ? awayStatus.UserName : null;
            }
        }
        private async Task StoreToken(string accessToken, string emailAddress)
        {
            using (IAwayStatusRepository awayStatusRepository = new AwayStatusRepository())
            {
                await awayStatusRepository.SetGoogleTokenAsync(emailAddress, accessToken);
            }
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
