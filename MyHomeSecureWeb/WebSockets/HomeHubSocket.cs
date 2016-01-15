using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;
using MyHomeSecureWeb.Utilities;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubSocket : IHomeHubSocket, IDisposable
    {
        private WebSocket _socket;

        private string _homeHubId;
        private CheckInOutMonitor _checkInOutMonitor;

        public HomeHubSocket(WebSocket socket)
        {
            _socket = socket;
            Debug.WriteLine("Conection opened");
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

                _homeHubId = value;
                _checkInOutMonitor = CheckInOutMonitor.Create(_homeHubId);
                _checkInOutMonitor.CheckInOut += _checkInOutMonitor_CheckInOut;

                using (var homeHubAwayChange = new HomeHubAwayChange(this))
                {
                    homeHubAwayChange.InitialiseHub();
                    LogConnectionMessage("Hub has connected");
                }
            }
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

        public async Task Process()
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            while (true)
            {
                WebSocketReceiveResult result = await _socket.ReceiveAsync(
                    buffer, CancellationToken.None);
                if (_socket.State == WebSocketState.Open)
                {
                    string message = Encoding.UTF8.GetString(
                        buffer.Array, 0, result.Count);

                    ReceivedMessage(message);
                }
                else
                {
                    break;
                }
            }
        }

        public void ReceivedMessage(string message)
        {
            var decoded = JsonConvert.DeserializeObject<SocketMessageBase>(message);

            var targetName = string.Format("MyHomeSecureWeb.WebSockets.HomeHub{0}", decoded.Method);
            var target = Activator.CreateInstance(Type.GetType(targetName), this) as ISocketTarget;
            using (target)
            {
                var methodInfo = target.GetType().GetMethod(decoded.Method);
                if (methodInfo != null)
                {
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length > 0)
                    {
                        var methodParams = new[]
                        {
                            JsonConvert.DeserializeObject(message, parameters[0].ParameterType)
                        };
                        methodInfo.Invoke(target, methodParams);
                    }
                }
            }
        }

        public void SendMessage<T>(T message)
        {
            SendMessage(JsonConvert.SerializeObject(message));
        }

        private void SendMessage(string message)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            buffer = new ArraySegment<byte>(
                Encoding.UTF8.GetBytes(message));
            _socket.SendAsync(
                buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public virtual void Dispose()
        {
            Debug.WriteLine("Conection closed");
            if (_checkInOutMonitor != null)
            {
                _checkInOutMonitor.CheckInOut -= _checkInOutMonitor_CheckInOut;
                _checkInOutMonitor.Dispose();
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
