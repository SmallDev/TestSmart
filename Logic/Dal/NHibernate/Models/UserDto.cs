using System;
using Logic.Model;

namespace Logic.Dal.NHibernate.Models
{
    public class UserDto
    {
        public virtual Int32 Id { get; set; }
        public virtual String Mac { get; set; }

        public static implicit operator User(UserDto user)
        {
            return new User
            {
                Id = user.Id,
                MacAddress = user.Mac
            };
        }
        public static implicit operator UserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Mac = user.MacAddress
            };
        }
    }
}