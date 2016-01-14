namespace MyHomeSecureWeb.Models
{
    public class HubInitialiseRequest : SocketMessageBase
    {
        public string Name { get; set; }
        public string Token { get; set; }
    }

    public class HubInitialiseResponse
    {
        public string Response { get; set; }
    }
}
