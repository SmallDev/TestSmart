﻿using System;
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

        public IList<Cluster> GetClusters()
        {
            return dataFactory.Value.WithRepository<IList<Cluster>, IClusterRepository>(
                repo => repo.GetList(new ClusterWith().WithSize()));
        }
        public Cluster GetCluster(Int32 id)
        {
            return dataFactory.Value.WithRepository<Cluster, IClusterRepository>(
                repo => repo.Get(id, new ClusterWith().WithSize()));
        }

        public IList<User> GetUsers(String macFilter, Int32 page, Int32 size)
        {
            return dataFactory.Value.WithRepository<IList<User>, IUserRepository>(
                repo => repo.GetUsers());
        }
    }
}
