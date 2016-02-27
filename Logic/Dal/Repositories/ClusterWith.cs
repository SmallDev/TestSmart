using System;

namespace Logic.Dal.Repositories
{
    public class ClusterWith
    {
        public Boolean WithSize { get; set; }
        public Boolean WithProperties { get; set; }        
    }

    public static class ClusterWithExtension
    {
        public static ClusterWith WithSize(this ClusterWith with)
        {
            with.WithSize = true;
            return with;
        }
        public static ClusterWith WithProperties(this ClusterWith with)
        {
            with.WithProperties = true;
            return with;
        }
    }
}