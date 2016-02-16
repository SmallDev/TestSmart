using System;

namespace Logic.Dal.Repositories
{
    public interface ISettingsRepository : IRepository
    {
        DateTime? GetReadTimestamp();
        void SetReadTimestamp(DateTime? dateTime);
    }
}