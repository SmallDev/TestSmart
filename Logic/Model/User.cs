using System;

namespace Logic.Model
{
    public class User : IEntity
    {
        public Int32 Id { get; set; }
        public String MacAddress { get; set; }
    }
}