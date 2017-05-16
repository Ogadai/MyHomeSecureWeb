﻿using Microsoft.WindowsAzure.Mobile.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Security.Principal;
using System.Threading;
using System.Net.Http;
using System.Net;

namespace MyHomeSecureWeb.Utilities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class GoogleAuthorisationAttribute : AuthorizationFilterAttribute
    {
        AuthorizationLevel _level;

        private static String _applicationKey = AppSettings.GetApplicationKey();

        public GoogleAuthorisationAttribute()
        {
            _level = AuthorizationLevel.Anonymous;
        }
        public GoogleAuthorisationAttribute(AuthorizationLevel level)
        {
            _level = level;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var headers = actionContext.Request.Headers;
            
            var appKey = headers.Contains("X-ZUMO-APPLICATION") ? headers.GetValues("X-ZUMO-APPLICATION").First() : "";
            var authToken = headers.Contains("X-ZUMO-AUTH") ? headers.GetValues("X-ZUMO-AUTH").FirstOrDefault() : "";

            if (_level != AuthorizationLevel.Anonymous &&
                (string.IsNullOrEmpty(appKey)
              || (!string.IsNullOrEmpty(_applicationKey) && string.Compare(appKey, _applicationKey) != 0)
              || string.IsNullOrEmpty(authToken)))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return;
            }

            if (!string.IsNullOrEmpty(authToken))
            {
                var identity = new GoogleAuthorisationIdentity(authToken);
                Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
            }

            base.OnAuthorization(actionContext);
        }
    }

    public class GoogleAuthorisationIdentity : GenericIdentity
    {
        public GoogleAuthorisationIdentity(string authToken)
                : base(authToken)
        {
            AuthToken = authToken;
        }

        /// <summary>
        /// Basic Auth Password for custom authentication
        /// </summary>
        public string AuthToken { get; private set; }
    }
}