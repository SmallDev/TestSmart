using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Logic.Facades;
using Logic.Model;
using WebClient.Models;

namespace WebClient.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly Lazy<StatisticsFacade> tempFacade;
        private readonly Lazy<DataFacade> facade;

        public HomeController(Func<DataFacade> facade, Func<StatisticsFacade> tempFacade)
        {
            this.tempFacade = new Lazy<StatisticsFacade>(tempFacade);
            this.facade = new Lazy<DataFacade>(facade);
        }

        public async virtual Task<ActionResult> Index()
        {
            tempFacade.Value.StartEmulate();
            var stat = new Statistics();
            while (stat.CalculatePersentage < 1)
            {
                Thread.Sleep(500);
                stat = tempFacade.Value.ReadStatistics();
                Debug.WriteLine("Read: {0}\tCalc: {1}", stat.ReadPercentage, stat.CalculatePersentage);
            }

            tempFacade.Value.StartEmulate();
            stat = new Statistics();
            while (stat.CalculatePersentage < 1)
            {
                Thread.Sleep(500);
                stat = tempFacade.Value.ReadStatistics();
                Debug.WriteLine("Read: {0}\tCalc: {1}", stat.ReadPercentage, stat.CalculatePersentage);
            }

            var model = new HomeModel();
            await Task.WhenAll(
                Task.Run(() => model.Clusters = facade.Value.GetClusters(1, 0).Select(c => new ClusterModel(c)).ToList()),
                Task.Run(() => model.Users = facade.Value.GetUsers(1, 0).Select(u => new UserModel(u)).ToList()));

            return View(MVC.Home.Views.Index, model);
        }
    }
}
