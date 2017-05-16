using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using MyHomeSecureWeb.Notifications;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.Controllers
{
    //[GoogleAuthorisation(AuthorizationLevel.Anonymous)]
    //public class TestController : ApiController
    //{
    //    public ApiServices Services { get; set; }

    //    private static string TestHubId = "<hub-id>";

    //    [HttpGet]
    //    [Route("api/test/notify")]
    //    public async Task<IHttpActionResult> notify(string state, bool active)
    //    {
    //        var statusNotification = new StateNotification(Services);
    //        await statusNotification.Send(TestHubId, state, active, "garage", "rule");

    //        var message = JsonConvert.SerializeObject(new StatusMessage
    //        {
    //            Message = "StateNotification",
    //            HomeHubId = TestHubId,
    //            State = state,
    //            Active = active,
    //            Node = "garage",
    //            Rule = "rule"
    //        });
    //        return Ok(string.Format("Sent {0}", message));
    //    }
    //}
}
