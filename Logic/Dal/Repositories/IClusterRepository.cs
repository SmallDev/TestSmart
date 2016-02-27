using System;
using System.Collections.Generic;
using Logic.Model;

namespace Logic.Dal.Repositories
{
    public interface IClusterRepository : IRepository
    {
        IList<Cluster> GetList(ClusterWith with);
        Cluster Get(Int32 id, ClusterWith with);

        void Save(Cluster cluster);        
    }
}