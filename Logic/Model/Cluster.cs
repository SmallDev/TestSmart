using System;

namespace Logic.Model
{
    public class Cluster : IEntity
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }

        public Double Size { get; set; }
    }
}
