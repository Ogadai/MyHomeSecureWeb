using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using System;
using System.Diagnostics;
using System.Net.WebSockets;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubSocket : SocketBase, IHomeHubSocket, IDisposable
    {
        private string _homeHubId;
        private CheckInOutMonitor _checkInOutMonitor;
        private ChatHub _chatHub;

        public HomeHubSocket(WebSocket socket) : base(socket)
        {
            Debug.WriteLine("HomeHub Conection opened");
        }

        public ChatHub ChatHub
        {
            get
            {
                return _chatHub;
            }
        }

        public string HomeHubId
        {
            get
            {
                return _homeHubId;
            }
            set
            {
                if (_checkInOutMonitor != null)
                {
                    _checkInOutMonitor.CheckInOut -= _checkInOutMonitor_CheckInOut;
                    _checkInOutMonitor.Dispose();
                    LogConnectionMessage("Hub has disconnected");
                }

                if (_chatHub != null)
                {
                    _chatHub.Dispose();
                    _chatHub.HomeMessage -= _chatHub_HomeMessage;
                }

                _homeHubId = value;

                _checkInOutMonitor = CheckInOutMonitor.Create(_homeHubId);
                _checkInOutMonitor.CheckInOut += _checkInOutMonitor_CheckInOut;

                _chatHub = ChatHub.Get(_homeHubId);
                _chatHub.HomeMessage += _chatHub_HomeMessage;

                using (var homeHubAwayChange = new HomeHubAwayChange(this))
                {
                    homeHubAwayChange.InitialiseHub();
                    LogConnectionMessage("Hub has connected");
                }
            }
        }

        private void _chatHub_HomeMessage(Models.SocketMessageBase message)
        {
            SendMessage(message);
        }

        private void _checkInOutMonitor_CheckInOut(string userName, bool away)
        {
            if (!string.IsNullOrEmpty(HomeHubId))
            {
                using (var homeHubAwayChange = new HomeHubAwayChange(this))
                {
                    homeHubAwayChange.UserCheckInOut(userName, away);
                }
            }
        }

        public override ISocketTarget CreateMessageInstance(Type type)
        {
            return Activator.CreateInstance(type, this) as ISocketTarget;
        }
        
        public virtual void Dispose()
        {
            Debug.WriteLine("HomeHub Conection closed");
            if (_checkInOutMonitor != null)
            {
                _checkInOutMonitor.CheckInOut -= _checkInOutMonitor_CheckInOut;
                _checkInOutMonitor.Dispose();
            }

            if (_chatHub != null)
            {
                _chatHub.HomeMessage -= _chatHub_HomeMessage;
                _chatHub.Dispose();
            }

            LogConnectionMessage("Hub has disconnected");
        }

        private void LogConnectionMessage(string message)
        {
            if (!string.IsNullOrEmpty(HomeHubId))
            {
                using (var logRepository = new LogRepository())
                {
                    logRepository.Info(HomeHubId, message);
                }
            }
        }
    }
}
