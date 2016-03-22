using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Logic.Model;

namespace WebClient.Models
{
    public class ClustersChartModel
    {
        public IList<Cluster> Clusters { get; set; }

        public bool ShowChart { get; set; }
    }
}