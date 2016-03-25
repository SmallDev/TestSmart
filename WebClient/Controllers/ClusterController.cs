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
        public virtual JsonResult GetClusters()
        {
            var clusters = statisticsFacade.Value.GetClusters();

            var pieClusters =  new List<ClusterPieModel>();
            if (clusters != null && clusters.Count > 0)
            {
                pieClusters = clusters.Select(cluster => new ClusterPieModel(cluster)).ToList();
            }
            
            var showChart = clusters != null && clusters.Count > 0 && clusters.Any(cluster => cluster.SizeHistory.Count > 0);

            return Json(new ClustersModel { PieClusters = pieClusters, ShowChart = showChart });
        }

        public virtual ActionResult Get(Int32 id)
        {
            var cluster = statisticsFacade.Value.GetCluster(id);
            return View("ClusterDetails", new ClusterModel(cluster));
        }
    }
}
