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
    // 1. Отображение статистики, отобразить на "градуснике"
    // StatisticsFacade.ReadStatistics

    // 2. Управление эмулятором: скорость и общее время чтения
    // EmulatorFacade.GetVelocity и EmulatorFacade.SetVelocity - работа со скоростью (1.0 - нормальная скорость, < 1 - замедление, > 1 - ускорение)
    // 
    public partial class HomeController : Controller
    {
        private readonly Lazy<EmulatorFacade> emulatorFacade;
        private readonly Lazy<LearningFacade> learningFacade;
        private readonly Lazy<StatisticsFacade> statisticsFacade;
        private readonly Lazy<DataFacade> facade;

        public HomeController(Func<DataFacade> facade, Func<StatisticsFacade> statisticsFacade, 
            Func<EmulatorFacade> emulatorFacade, Func<LearningFacade> learningFacade)
        {
            this.emulatorFacade = new Lazy<EmulatorFacade>(emulatorFacade);
            this.learningFacade = new Lazy<LearningFacade>(learningFacade);
            this.statisticsFacade = new Lazy<StatisticsFacade>(statisticsFacade);
            this.facade = new Lazy<DataFacade>(facade);
        }

        public async virtual Task<ActionResult> Index()
        {
            var cls = statisticsFacade.Value.GetClusters();
            var cl = statisticsFacade.Value.GetCluster(1);

            var uu = statisticsFacade.Value.GetUsers("00", 1, 10);
            var u1 = statisticsFacade.Value.GetUser(1);
            var allu = statisticsFacade.Value.GetUsers("00");
            
            var model = new HomeModel();
            await Task.WhenAll(
                Task.Run(() => model.Clusters = facade.Value.GetClusters(1, 0).Select(c => new ClusterModel(c)).ToList()),
                Task.Run(() => model.Users = facade.Value.GetUsers(1, 0).Select(u => new UserModel(u)).ToList()));

            return View(MVC.Home.Views.Index, model);
        }

        public ActionResult GetControlPanel()
        {
            return View("ControlPanel", GetControlData());
        }

        public JsonResult GetControlData()
        {
            return Json(new { velocity = 2, allTime = "00:10:00", calcTime = "00:01:00", readTime= "00:08:00" });
        }
    }
}
