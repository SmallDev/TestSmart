using System.Collections.Generic;

namespace WebClient.Models
{
    public class HomeModel
    {
        public List<ClusterModel> Clusters { get; set; }
        public List<UserModel> Users { get; set; }
    }
}