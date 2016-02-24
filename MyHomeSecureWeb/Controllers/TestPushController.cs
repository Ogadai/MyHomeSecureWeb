using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class TestPushController : ApiController
    {
        public ApiServices Services { get; set; }

        public async Task<IHttpActionResult> GetTestPush()
        {
            IStateNotification notify = new StateNotification(Services);
            await notify.Send("blah", "Alert", true);

            return Ok();
        }
    }
}
