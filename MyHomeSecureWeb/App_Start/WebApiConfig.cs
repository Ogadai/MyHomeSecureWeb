using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using Microsoft.WindowsAzure.Mobile.Service;
using System.Net.Http.Headers;
using MyHomeSecureWeb.Utilities;

namespace MyHomeSecureWeb
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));
            config.EnableCors();
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            Database.SetInitializer(new MobileServiceInitializer());
        }
    }

    public class MobileServiceInitializer : DropCreateDatabaseIfModelChanges<MobileServiceContext>
    {
        private PasswordHash _passwordHash = new PasswordHash();

        protected override void Seed(MobileServiceContext context)
        {
            var salt = _passwordHash.CreateSalt(32);
            var tokenHash = _passwordHash.Hash("I am a test hub", salt);
            var hub = new HomeHub
            {
                Id = Guid.NewGuid().ToString(),
                TokenHash = tokenHash,
                TokenSalt = salt
            };
            context.Set<HomeHub>().Add(hub);

            var awayStatuses = new List<AwayStatus>
            {
                CreateAwayStatus(hub.Id, "andy.lee.surfer@gmail.com", "I am a test user"),
                CreateAwayStatus(hub.Id, "bexslee3@gmail.com", "I am a test user")
            };

            foreach (var awayStatus in awayStatuses)
            {
                context.Set<AwayStatus>().Add(awayStatus);
            }

            base.Seed(context);
        }

        private AwayStatus CreateAwayStatus(string hubId, string userName, string token)
        {
            var salt = _passwordHash.CreateSalt(32);
            var tokenHash = _passwordHash.Hash(token, salt);

            return new AwayStatus
            {
                Id = Guid.NewGuid().ToString(),
                HomeHubId = hubId,
                UserName = userName,
                Away = false,
                TokenHash = tokenHash,
                TokenSalt = salt
            };
        }
    }
}

