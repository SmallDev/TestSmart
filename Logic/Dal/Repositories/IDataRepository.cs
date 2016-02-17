using System.Collections.Generic;
using Logic.Model;

namespace Logic.Dal.Repositories
{
    public interface IDataRepository : IRepository
    {
        void Save(IList<Data> data);
        void Clear();
    }
}