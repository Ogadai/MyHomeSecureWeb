using MyHomeSecureWeb.DataObjects;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public interface IAwayStatusRepository: IDisposable
    {
        AwayStatus GetStatus(string userName);
        void UpdateStatus(string userName, bool away);
        void SetToken(string userName, byte[] tokenHash);
        void AddUser(string userName, string homeHubId, byte[] tokenHash, byte[] salt);
        void RemoveUser(string userName);
        IQueryable<AwayStatus> GetAllForHub(string homeHubId);
    }
}