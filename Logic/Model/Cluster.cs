using System;
using System.Collections.Generic;

namespace Logic.Model
{
    public class Cluster : IEntity
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }

        public ICollection<Tuple<TimeSpan, Double>> SizeHistory { get; set; }
        public ICollection<Tuple<User, Double>> UsersInfo { get; set; }
        public ICollection<Property> Properties { get; set; } 
    }
}
