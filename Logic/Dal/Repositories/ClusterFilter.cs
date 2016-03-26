using System;

namespace Logic.Dal.Repositories
{
    public class ClusterFilter
    {
        public Int32? Id { get; set; }
        public Boolean WithSize { get; set; }
        public Boolean WithProperties { get; set; }
        public Boolean WithUsers { get; set; }
    }

    public static class ClusterFilterExtension
    {
        public static ClusterFilter Id(this ClusterFilter filter, Int32 id)
        {
            filter.Id = id;
            return filter;
        }
        public static ClusterFilter WithSize(this ClusterFilter filter)
        {
            filter.WithSize = true;
            return filter;
        }
        public static ClusterFilter WithUsers(this ClusterFilter filter)
        {
            filter.WithUsers = true;
            return filter;
        }
        public static ClusterFilter WithProperties(this ClusterFilter filter)
        {
            filter.WithProperties = true;
            return filter;
        }
        public static ClusterFilter WithAll(this ClusterFilter filter)
        {
            return filter.WithSize().WithUsers().WithProperties();
        }
    }
}