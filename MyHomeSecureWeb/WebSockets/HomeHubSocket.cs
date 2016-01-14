using MyHomeSecureWeb.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.WebSockets;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubSocket : SocketBase
    {
        public HomeHubSocket(WebSocket socket) : base(socket)
        {
        }

        public void Initialise(HubInitialiseRequest request)
        {
            Debug.WriteLine(string.Format("Initialise hub: {0}", request.Name));

            SendMessage(new HubInitialiseResponse { Response = string.Format("You initialised hub: " + request.Name + " with token " + request.Token) } );
        }
    }
}
