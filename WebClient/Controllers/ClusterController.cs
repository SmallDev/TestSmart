using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Logic.Facades;
using Logic.Model;
using System.Linq;
using WebClient.Models;

namespace WebClient.Controllers
{
    public partial class ClusterController : Controller
    {
        private readonly Lazy<StatisticsFacade> statisticsFacade;
        public ClusterController(Func<StatisticsFacade> statisticsFacade)
        {
            this.statisticsFacade = new Lazy<StatisticsFacade>(statisticsFacade);
        }

        public virtual ActionResult GetList()
        {
            return View(MVC.Cluster.Views.Index);
        }

        [HttpPost]
        public JsonResult GetClusters()
        {
            var clusters = statisticsFacade.Value.GetClusters();
            
            var showChart = clusters != null && clusters.Count > 0 && clusters.Any(cluster => cluster.Size > 0);

            return Json(new ClustersChartModel { Clusters = clusters, ShowChart = showChart});
        }

        public virtual ActionResult Get(Int32 id)
        {
            var cluster = statisticsFacade.Value.GetCluster(id);
            return View("ClusterDetails", cluster);
        }
    }
}
