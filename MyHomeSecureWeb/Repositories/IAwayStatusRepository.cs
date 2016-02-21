using MyHomeSecureWeb.DataObjects;
using System;
using System.Linq;

namespace MyHomeSecureWeb.Repositories
{
    public interface IAwayStatusRepository: IDisposable
    {
        AwayStatus GetStatus(string userName);
        AwayStatus GetStatusFromGoogleToken(string token);
        void UpdateStatus(string userName, bool away);
        void SetGoogleToken(string userName, string token);
        void SetToken(string userName, byte[] tokenHash);
        void AddUser(string userName, string homeHubId, byte[] tokenHash, byte[] salt);
        void RemoveUser(string userName);
        IQueryable<AwayStatus> GetAllForHub(string homeHubId);
    }
}