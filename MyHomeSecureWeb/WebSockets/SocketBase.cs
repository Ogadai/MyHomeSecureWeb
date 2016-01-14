using MyHomeSecureWeb.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyHomeSecureWeb.WebSockets
{
    public abstract class SocketBase : IDisposable
    {
        private WebSocket _socket;
        public SocketBase(WebSocket socket)
        {
            _socket = socket;
            Debug.WriteLine("Conection opened");
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

            var methodInfo = GetType().GetMethod(decoded.Method);
            if (methodInfo != null)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length > 0)
                {
                    var methodParams = new[]
                    {
                        JsonConvert.DeserializeObject(message, parameters[0].ParameterType)
                    };
                    methodInfo.Invoke(this, methodParams);
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
        }
    }
}
