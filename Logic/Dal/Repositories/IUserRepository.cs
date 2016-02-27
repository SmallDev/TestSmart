using System.Collections.Generic;
using Logic.Model;

namespace Logic.Dal.Repositories
{
    public interface IUserRepository : IRepository
    {
        User GetUser(UserFilter filter);
        IList<User> GetUsers(UserFilter filter);
    }
}
