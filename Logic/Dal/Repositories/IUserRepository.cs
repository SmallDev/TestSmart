using System.Collections.Generic;
using Logic.Model;

namespace Logic.Dal.Repositories
{
    public interface IUserRepository : IRepository
    {
        IList<User> GetUsers();
    }
}
