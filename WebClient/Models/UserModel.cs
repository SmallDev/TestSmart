using System;
using Logic.Model;

namespace WebClient.Models
{
    public class UserModel
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }

        public UserModel(User user)
        {
            Id = user.Id;
            Name = user.MacAddress;
        }
    }
}