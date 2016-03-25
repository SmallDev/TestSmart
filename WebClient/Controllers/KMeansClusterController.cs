using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Logic.Facades;
using WebClient.Models;

namespace WebClient.Controllers
{
    public partial class KMeansClusterController : Controller
    {
        private readonly Lazy<StatisticsFacade> statisticsFacade;
        public KMeansClusterController(Func<StatisticsFacade> statisticsFacade)
        {
            this.statisticsFacade = new Lazy<StatisticsFacade>(statisticsFacade);
        }

        public virtual ActionResult GetList()
        {
            return View(MVC.KMeansCluster.Views.Index);
        }

        [HttpPost]
        public virtual JsonResult GetClusters()
        {
            var clusters = statisticsFacade.Value.GetKMeansClusters();

            var pieClusters = new List<ClusterPieModel>();
            if (clusters != null && clusters.Count > 0)
            {
                pieClusters = clusters.Select(cluster => new ClusterPieModel(cluster)).ToList();
            }

            var showChart = clusters != null && clusters.Count > 0 && clusters.Any(cluster => cluster.SizeHistory.Count > 0);

            return Json(new ClustersModel { PieClusters = pieClusters, ShowChart = showChart });
        }

        public virtual ActionResult Get(Int32 id)
        {
            var cluster = statisticsFacade.Value.GetKMeansCluster(id);
            return View(MVC.KMeansCluster.Views.ClusterDetails, new ClusterModel(cluster));
        }
    }
}
