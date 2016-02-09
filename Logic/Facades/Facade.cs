using System;
using System.Collections.Generic;
using Logic.Dal;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Facades
{
    public class Facade
    {
        private readonly IDataManader dataManager;
        public Facade(IDataManader dataManager)
        {
            this.dataManager = dataManager;
        }

        public ICollection<User> GetUsers(Int32 page, Int32 size)
        {
            var users = dataManager.GetRepository<IUserRepository>().GetUsers();
            return users;
        }
    }
}
