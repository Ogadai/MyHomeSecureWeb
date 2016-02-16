using System.Web.WebSockets;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using System.Net.WebSockets;
using MyHomeSecureWeb.Utilities;
using MyHomeSecureWeb.WebSockets;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RequireHttps]
    public class HomeHubController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            HttpContext currentContext = HttpContext.Current;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {

                currentContext.AcceptWebSocketRequest(ProcessWSChat, new AspNetWebSocketOptions { SubProtocol = "echo-protocol" });
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        private async Task ProcessWSChat(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            using (var homeHubSocket = new HomeHubSocket(context.WebSocket))
            {
                await homeHubSocket.Process();
            }
        }
    }
}
