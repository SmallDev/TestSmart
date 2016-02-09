using System.Collections.Generic;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Dal.Wcf
{
    class UserWcfRepository : IUserRepository
    {
        public ICollection<User> GetUsers()
        {
            throw new System.NotImplementedException();
        }
    }
}