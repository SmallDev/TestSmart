﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            Properties = cluster.Properties.Select(p =>
                new KeyValuePair<String, String>(p.Name, Convert.ToString(p.Mean))).ToList();
        }

        public string Name { get; set; }

        public List<List<UserModel>>  Users { get; set; }
        public List<KeyValuePair<String, String>> Properties { get; set; }
 
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