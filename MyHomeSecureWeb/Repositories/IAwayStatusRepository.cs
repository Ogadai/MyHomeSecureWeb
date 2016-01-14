using MyHomeSecureWeb.DataObjects;
using System;

namespace MyHomeSecureWeb.Repositories
{
    public interface IAwayStatusRepository: IDisposable
    {
        AwayStatus GetStatus(string userName);
        void UpdateStatus(string userName, bool away);
    }
}