using System.Web.Configuration;

namespace MyHomeSecureWeb.Utilities
{
    public class AppSettings
    {
        public static string GetClientId()
        {
            return WebConfigurationManager.AppSettings["ClientId"];
        }
        public static string GetClientSecret()
        {
            return WebConfigurationManager.AppSettings["ClientSecret"];
        }

    }
}
