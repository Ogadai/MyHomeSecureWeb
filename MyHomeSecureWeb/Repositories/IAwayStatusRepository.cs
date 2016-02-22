using MyHomeSecureWeb.DataObjects;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public interface IAwayStatusRepository: IDisposable
    {
        AwayStatus GetStatus(string userName);
        void UpdateStatus(string userName, bool away);
        void SetToken(string userName, byte[] tokenHash, byte[] salt);
        void AddUser(string userName, string homeHubId);
        void RemoveUser(string userName);
        IQueryable<AwayStatus> GetAllForHub(string homeHubId);
    }
}