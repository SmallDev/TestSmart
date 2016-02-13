﻿using System;
using Logic.Model;

namespace WebClient.Models
{
    public class ClusterModel
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public Int32 UsersCount { get; set; }

        public ClusterModel(Cluster cluster)
        {
            Id = cluster.Id;
            Name = cluster.Name;
            UsersCount = cluster.UsersCount;
        }
    }
}