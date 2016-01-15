namespace MyHomeSecureWeb.WebSockets
{
    public interface ISocketSender
    {
        void SendMessage<T>(T message);
    }
}
