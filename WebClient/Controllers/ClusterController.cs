using System;
using System.Web.Mvc;
using Logic.Facades;

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
            var clusters = statisticsFacade.Value.GetClusters();
            return View(MVC.Cluster.Views.Index, clusters);
        }
        public virtual ActionResult Get(Int32 id)
        {
            var cluster = statisticsFacade.Value.GetCluster(id);
            return View(MVC.Cluster.Views.Index, cluster);
        }
    }
}
