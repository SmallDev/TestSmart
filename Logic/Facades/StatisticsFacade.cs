using System;
using System.Collections.Generic;
using System.Linq;
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
                repo => repo.GetList(new ClusterFilter().Id(id).WithAll()).FirstOrDefault());
        }

        public IList<Cluster> GetKMeansClusters()
        {
            return dataFactory.Value.WithRepository<IList<Cluster>, IHiveClusterRepository>(
                repo => repo.GetList(new ClusterFilter().WithSize()));
        }
        public Cluster GetKMeansCluster(Int32 id)
        {
            return dataFactory.Value.WithRepository<Cluster, IHiveClusterRepository>(
                repo => repo.GetList(new ClusterFilter().Id(id).WithAll()).FirstOrDefault());
        }
    }
}
