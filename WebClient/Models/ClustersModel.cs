using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Logic.Model;

namespace WebClient.Models
{
    public class ClustersModel
    {
        public IList<ClusterPieModel> PieClusters { get; set; }

        public bool ShowChart { get; set; }
    }
}