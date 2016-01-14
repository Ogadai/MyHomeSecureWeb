using System.Web.WebSockets;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace MyHomeSecureWeb.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class HomeHubController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            HttpContext currentContext = HttpContext.Current;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                currentContext.AcceptWebSocketRequest(ProcessWSChat);
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        private async Task ProcessWSChat(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            Debug.WriteLine("Conection opened");
            while (true)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(
                    buffer, CancellationToken.None);
                if (socket.State == WebSocketState.Open)
                {
                    string userMessage = Encoding.UTF8.GetString(
                        buffer.Array, 0, result.Count);

                    Debug.WriteLine(string.Format("Message received: {0}", userMessage));

                    userMessage = "You sent: " + userMessage + " at " +
                        DateTime.Now.ToLongTimeString();
                    buffer = new ArraySegment<byte>(
                        Encoding.UTF8.GetBytes(userMessage));
                    await socket.SendAsync(
                        buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    break;
                }
            }
            Debug.WriteLine("Conection closed");
        }
    }
}
