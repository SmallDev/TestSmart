using System.Collections.Generic;
using Logic.Model;

namespace Logic.Dal.Repositories
{
    public interface IClusterRepository : IRepository
    {
        IList<Cluster> GetList(ClusterFilter filter);
        Cluster Get(ClusterFilter filter);

        void Save(Cluster cluster);        
    }
}