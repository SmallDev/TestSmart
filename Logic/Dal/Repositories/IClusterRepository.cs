using System;
using System.Collections.Generic;
using Logic.Model;

namespace Logic.Dal.Repositories
{
    public interface IClusterRepository : IRepository
    {
        IList<Cluster> GetList(ClusterFilter filter);       
    }
    public interface IHiveClusterRepository : IRepository
    {
        IList<Cluster> GetList(Int32 allCount, ClusterFilter filter);
    }
}