using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Dal;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Facades
{
    public class Facade
    {
        private readonly IDataManagerFactrory dataFactory;
        public Facade(IDataManagerFactrory dataFactory)
        {
            this.dataFactory = dataFactory;
        }

        public ICollection<User> GetUsers(Int32 page, Int32 size)
        {
            using (var dataManager = dataFactory.GetDataManager())
            {
                var t = dataManager.GetRepository<IUserRepository>().GetUsers();
                var t2 = dataManager.GetRepository<IClusterRepository>();
            }

            //var users = dataManager.GetRepository<IUserRepository>().GetUsers();
            //return users;
            return Enumerable.Range(1, 10)
                .Select(i => new User {Id = i, MacAddress = "Пользователь " + i})
                .ToList();
        }

        public ICollection<Cluster> GetClusters(Int32 page, Int32 size)
        {
            return Enumerable.Range(1, 5)
                .Select(i => new Cluster {Id = i, Name = "Кластер " + i, UsersCount = 5})
                .ToList();
        }
    }
}
