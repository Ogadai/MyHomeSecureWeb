using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Notifications;
using MyHomeSecureWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.Controllers
{
    //[AuthorizeLevel(AuthorizationLevel.Anonymous)]
    //public class TestPushController : ApiController
    //{
    //    public ApiServices Services { get; set; }

    //    public async Task<IHttpActionResult> GetTestPush()
    //    {
    //        var hubId = "";
    //        using (IHomeHubRepository homeHubRepository = new HomeHubRepository())
    //        {
    //            var hub = homeHubRepository.GetHub("OgadaiMansion");
    //            if (hub != null)
    //            {
    //                hubId = hub.Id;
    //            }
    //        }

    //            IStateNotification notify = new StateNotification(Services);
    //        await notify.Send(hubId, "Alert", true);

    //        return Ok();
    //    }
    //}
}
