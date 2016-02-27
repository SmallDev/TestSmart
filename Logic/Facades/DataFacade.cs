using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Dal;
using Logic.Model;

namespace Logic.Facades
{
    public class DataFacade
    {
        private readonly Lazy<IDataManagerFactrory> dataFactory;
        public DataFacade(Func<IDataManagerFactrory> dataFactory)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
        }

        public ICollection<User> GetUsers(Int32 page, Int32 size)
        {
            //return dataFactory.Value.WithRepository<ICollection<User>, IUserRepository>(
            //    repo => repo.GetUsers());
            
            //var users = dataManager.GetRepository<IUserRepository>().GetUsers();
            //return users;
            return Enumerable.Range(1, 10)
                .Select(i => new User {Id = i, MacAddress = "Пользователь " + i})
                .ToList();
        }

        public ICollection<Cluster> GetClusters(Int32 page, Int32 size)
        {
            return Enumerable.Range(1, 5)
                .Select(i => new Cluster {Id = i, Name = "Кластер " + i, Size = 5})
                .ToList();
        }
    }
}
