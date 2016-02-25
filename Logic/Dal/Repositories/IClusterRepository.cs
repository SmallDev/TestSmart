using System.Collections.Generic;
using Logic.Model;

namespace Logic.Dal.Repositories
{
    public interface IClusterRepository : IRepository
    {
        void Save(IList<Cluster> clusters);
    }
}