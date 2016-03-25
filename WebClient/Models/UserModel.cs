using System;
using Logic.Model;

namespace WebClient.Models
{
    public class UserModel
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public Double Weight { get; set; }


        public UserModel(User user, Double prob)
        {
            Id = user.Id;
            Name = user.MacAddress;
            Weight = Math.Round(prob*100, 2);
        }
    }
}