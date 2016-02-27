using System;

namespace Logic.Dal.Repositories
{
    public class UserFilter
    {
        public Int32? Id { get; set; }
        public String Mac { get; set; }
        public Int32 PageNumber { get; set; }
        public Int32 PageSize { get; set; }
    }

    public static class UserFilterExtension
    {
        public static UserFilter Id(this UserFilter filter, Int32 id)
        {
            filter.Id = id;
            return filter;
        }
        public static UserFilter Mac(this UserFilter filter, String mac)
        {
            filter.Mac = mac;
            return filter;
        }

        public static UserFilter PageNumber(this UserFilter filter, Int32 number)
        {
            filter.PageNumber = number;
            return filter;
        }

        public static UserFilter PageSize(this UserFilter filter, Int32 size)
        {
            filter.PageSize = size;
            return filter;
        }
    }
}