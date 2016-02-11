using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;

namespace MyHomeSecureWeb.WebSockets
{
    public class UserAppSocket : SocketBase, IUserAppSocket, IDisposable
    {
        private ChatHub _chatHub;

        public UserAppSocket(WebSocket socket, string homeHubId) : base(socket)
        {
            Debug.WriteLine("UserApp Conection opened");

            _chatHub = ChatHub.Get(homeHubId);
            _chatHub.ClientMessage += _chatHub_ClientMessage;

            SendInitialStates(homeHubId);
        }

        private void _chatHub_ClientMessage(SocketMessageBase message)
        {
            SendMessage(message);
        }

        public override ISocketTarget CreateMessageInstance(Type type)
        {
            return Activator.CreateInstance(type, this) as ISocketTarget;
        }

        private void SendInitialStates(string homeHubId)
        {
            using (var hubStateRepository = new HubStateRepository())
            {
                var hubStates = hubStateRepository.GetAllForHub(homeHubId);
                var messageStates = new HubChangeStates {
                    States = hubStates.Select((s) => new HubChangeState {
                        Name = s.Name,
                        Active = s.Active
                    }).ToArray()
                };

                SendMessage(messageStates);
            }
        }

        public void Dispose()
        {
            _chatHub.Dispose();
        }
    }
}
