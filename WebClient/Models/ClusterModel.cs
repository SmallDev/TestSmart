using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Common;
using Logic.Model;

namespace WebClient.Models
{
    public class ClusterModel
    {
        public ClusterModel(Cluster cluster)
        {
            Name = cluster.Name;
            var allUserModels = cluster.UsersInfo.Select(userInf => new UserModel(userInf.Item1, userInf.Item2)).ToList();
            Users = SplitUsers(allUserModels);
            Properties = cluster.Properties.Select(p => new PropertyModel(p)).ToList();
            if (cluster.SizeHistory != null && cluster.SizeHistory.Count > 0)
                ClusterSize = Math.Round(cluster.SizeHistory.Last().Item2, 2);
        }

        public string Name { get; set; }

        public double ClusterSize { get; set; }

        public List<List<UserModel>>  Users { get; set; }
        public List<PropertyModel> Properties { get; set; }
 
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

    public class PropertyModel
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public String Value { get; set; }

        public PropertyModel(Property property)
        {
            Name = property.Type.ToString();
            Description = property.Type.GetDescription();
            Value = Convert.ToString(Math.Round(property.Mean, 2));
        }
    }
}