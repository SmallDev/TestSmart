using System;

namespace Logic.Dal.Repositories
{
    public interface ISettingsRepository : IRepository
    {
        TimeSpan? GetReadTime();
        void SetReadTime(TimeSpan? time);

        Double GetReadVelocity();
        void SetReadVelocity(Double velocity);

        TimeSpan? GetCalcTime();
        void SetCalcTime(TimeSpan? time);

        TimeSpan? GetAllTime();
        void SetAllTime(TimeSpan? time);

    }
}