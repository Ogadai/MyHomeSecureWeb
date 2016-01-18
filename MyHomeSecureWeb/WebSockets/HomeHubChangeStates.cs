using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubChangeStates : ISocketTarget
    {
        private IHomeHubSocket _homeHubSocket;
        private IHubStateRepository _hubStateRepository = new HubStateRepository();
        private ILogRepository _logRepository = new LogRepository();

        public HomeHubChangeStates(IHomeHubSocket homeHubSocket)
        {
            _homeHubSocket = homeHubSocket;
        }

        public void ChangeStates(HubChangeStates states)
        {
            if (!string.IsNullOrEmpty(_homeHubSocket.HomeHubId))
            {
                foreach(var state in states.States)
                {
                    _hubStateRepository.SetState(_homeHubSocket.HomeHubId, state.Name, state.Active);
                    _logRepository.Info(_homeHubSocket.HomeHubId,
                        string.Format("{0} changed to {1}", state.Name, state.Active ? "Active" : "Inactive"));
                }
            }
        }

        public void Dispose()
        {
            _hubStateRepository.Dispose();
            _logRepository.Dispose();
        }
    }
}
