using System;

namespace Logic.Dal
{
    public interface IDataManager : IDisposable
    {
        T GetRepository<T>() where T : IRepository;

        void Commit();
        void Rollback();
    }
}
