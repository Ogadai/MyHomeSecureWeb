namespace MyHomeSecureWeb.Models
{
    public class HubInitialiseRequest : SocketMessageBase
    {
        public string Name { get; set; }
        public string Token { get; set; }

        public HubInitialiseUser[] Users { get; set; }
        public string[] States { get; set; }
    }

    public class HubInitialiseUser : SocketMessageBase
    {
        public string Name { get; set; }
        public string Token { get; set; }
    }

    public class HubSetInitialStates : SocketMessageBase
    {
        public HubState[] States { get; set; }
    }

    public class HubState
    {
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}
