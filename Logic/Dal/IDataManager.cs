using System;
using Logic.Dal.Repositories;

namespace Logic.Dal
{
    public interface IDataManager : IDisposable
    {
        T GetRepository<T>() where T : IRepository;

        void Commit();
        void Rollback();
    }

    public static class DataManagerExtension
    {
        public static TRes WithDataManager<TRes>(this IDataManagerFactrory factory, Func<IDataManager, TRes> func, Boolean transaction = false)
        {
            using (var dataManager = factory.GetDataManager())
            {
                var result = func(dataManager);

                if (transaction)
                    dataManager.Commit();

                return result;
            }
        }
        public static void WithDataManager(this IDataManagerFactrory factory, Action<IDataManager> func, Boolean transaction = false)
        {
            factory.WithDataManager(manager =>
            {
                func(manager);
                return true;
            }, transaction);
        }

        public static TRes WithRepository<TRes, TRepo>(this IDataManagerFactrory factory, Func<TRepo, TRes> func, Boolean transaction = false) where TRepo : IRepository
        {
            return factory.WithDataManager(manager => func(manager.GetRepository<TRepo>()), transaction);
        }
        public static void WithRepository<TRepo>(this IDataManagerFactrory factory, Action<TRepo> func, Boolean transaction = false) where TRepo : IRepository
        {
            factory.WithRepository<Boolean, TRepo>(repo =>
            {
                func(repo);
                return true;
            }, transaction);
        }

        public static TRes WithRepository<TRes, TRepo>(this IDataManager manager, Func<TRepo, TRes> func) where TRepo : IRepository
        {
            return func(manager.GetRepository<TRepo>());
        }
        public static void WithRepository<TRepo>(this IDataManager manager, Action<TRepo> func) where TRepo : IRepository
        {
            manager.WithRepository<Boolean, TRepo>(repo =>
            {
                func(repo);
                return true;
            });
        }
    }
}
