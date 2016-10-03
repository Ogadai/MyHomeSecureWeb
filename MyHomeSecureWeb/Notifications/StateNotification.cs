using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyHomeSecureWeb.Notifications
{
    public class StateNotification : IStateNotification
    {
        private ApiServices _services;
        public StateNotification(ApiServices services)
        {
            _services = services;
        }

        public async Task Send(string homeHubId, string state, bool active, string node, string rule)
        {
            // Don't send if not activating state
            if (!active) return;

            var statusMessage = new StatusMessage {
                Message = "StateNotification",
                HomeHubId = homeHubId,
                State = state,
                Active = active,
                Node = node,
                Rule = rule
            };
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "message", JsonConvert.SerializeObject(statusMessage) }
            };
            GooglePushMessage message = new GooglePushMessage(data, TimeSpan.FromHours(1));

            try
            {
                var result = await _services.Push.SendAsync(message, homeHubId);
                _services.Log.Info(result.State.ToString());
            }
            catch (Exception ex)
            {
                _services.Log.Error(ex.Message, null, "Push.SendAsync Error");
            }
        }
    }
}
