using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Logic.Model;

namespace WebClient.Models
{
    public class ClusterModel
    {
        public ClusterModel(Cluster cluster)
        {
            Name = cluster.Name;
            var allUserModels = cluster.UsersInfo.Select(userInf => new UserModel(userInf.Item1)).ToList();
            Users = SplitUsers(allUserModels);
        }

        public string Name { get; set; }

        public List<List<UserModel>>  Users { get; set; }

        private List<List<UserModel>> SplitUsers(List<UserModel> users, int nSize = 10)
        {
            var list = new List<List<UserModel>>();

            for (int i = 0; i < users.Count; i += nSize)
            {
                list.Add(users.GetRange(i, Math.Min(nSize, users.Count - i)));
            }

            return list;
        }
    }
}