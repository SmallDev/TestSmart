using System;
using System.Collections.Generic;
using Logic.Dal;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Facades
{
    public class StatisticsFacade
    {
        private readonly Lazy<IDataManagerFactrory> dataFactory;
        public StatisticsFacade(Func<IDataManagerFactrory> dataFactory)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
        }

        public Statistics ReadStatistics()
        {
            var result = new Statistics();
            dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
            {
                result.AllTime = repo.GetAllTime() ?? TimeSpan.Zero;
                result.ReadTime = repo.GetReadTime() ?? TimeSpan.Zero;
                result.CalcTime = repo.GetCalcTime() ?? TimeSpan.Zero;
            });

            return result;         
        }

        public void InitClusters(Int32 count)
        {
            dataFactory.Value.WithRepository<ILearningRepository>(repo =>
            {
                repo.Clear();
                repo.InitClusters(count);
            });
        }

        public IList<Cluster> GetClusters()
        {
            return dataFactory.Value.WithRepository<IList<Cluster>, IClusterRepository>(
                repo => repo.GetList(new ClusterFilter().WithSize()));
        }
        public Cluster GetCluster(Int32 id)
        {
            return dataFactory.Value.WithRepository<Cluster, IClusterRepository>(
                repo => repo.Get(new ClusterFilter().Id(id).WithSize()));
        }

        public User GetUser(Int32 id)
        {
            return dataFactory.Value.WithRepository<User, IUserRepository>(
                repo => repo.GetUser(new UserFilter().Id(id)));
        }
        public IList<User> GetUsers(String macFilter, Int32 page = 1, Int32 size = 0)
        {
            return dataFactory.Value.WithRepository<IList<User>, IUserRepository>(
                repo => repo.GetUsers(new UserFilter().Mac(macFilter).PageNumber(page).PageSize(size)));
        }
    }
}
