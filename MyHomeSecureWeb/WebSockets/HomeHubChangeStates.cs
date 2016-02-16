using System;
using System.Linq;
using MyHomeSecureWeb.Models;
using MyHomeSecureWeb.Repositories;

namespace MyHomeSecureWeb.WebSockets
{
    public class HomeHubChangeStates : ISocketTarget
    {
        private IHomeHubSocket _homeHubSocket;
        private IHubStateRepository _hubStateRepository = new HubStateRepository();
        private ILogRepository _logRepository = new LogRepository();

        private string[] _priorityStates = new[] { "Away", "Alert", "Alarm" };

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
                    var changed = _hubStateRepository.SetState(_homeHubSocket.HomeHubId, state.Name, state.Active);

                    if (changed)
                    {
                        string severity = _priorityStates.Contains(state.Name) ? "Priority" : "Info";
                        _logRepository.LogEntry(_homeHubSocket.HomeHubId, severity,
                            string.Format("{0} changed to {1}", state.Name, state.Active ? "Active" : "Inactive"));
                    }
                }

                _homeHubSocket.ChatHub.MessageToClients(states);
            }
        }

        public void Dispose()
        {
            _hubStateRepository.Dispose();
            _logRepository.Dispose();
        }
    }
}
