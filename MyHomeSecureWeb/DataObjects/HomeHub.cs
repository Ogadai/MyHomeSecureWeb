using Microsoft.WindowsAzure.Mobile.Service;
using System.Collections.Generic;

namespace MyHomeSecureWeb.DataObjects
{
    public class HomeHub : EntityData
    {
        public string Name { get; set; }

        public byte[] TokenHash { get; set; }
        public byte[] TokenSalt { get; set; }

        public virtual ICollection<AwayStatus> AwayStatus { get; set; }
        public virtual ICollection<LogEntry> LogEntries { get; set; }
        public virtual ICollection<HubState> States { get; set; }
    }
}
