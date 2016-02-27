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
        private readonly Lazy<StatisticsFacade> tempFacade;
        private readonly Lazy<DataFacade> facade;

        public HomeController(Func<DataFacade> facade, Func<StatisticsFacade> tempFacade, 
            Func<EmulatorFacade> emulatorFacade, Func<LearningFacade> learningFacade)
        {
            this.emulatorFacade = new Lazy<EmulatorFacade>(emulatorFacade);
            this.learningFacade = new Lazy<LearningFacade>(learningFacade);
            this.tempFacade = new Lazy<StatisticsFacade>(tempFacade);
            this.facade = new Lazy<DataFacade>(facade);
        }

        public async virtual Task<ActionResult> Index()
        {
            var cl = tempFacade.Value.GetClusters();
            var ccc = tempFacade.Value.GetCluster(1);

            //tempFacade.Value.StartEmulate();
            //var stat = new Statistics();
            //while (stat.CalculatePersentage < 1)
            //{
            //    Thread.Sleep(500);
            //    stat = tempFacade.Value.ReadStatistics();
            //    Debug.WriteLine("Read: {0}\tCalc: {1}", stat.ReadPercentage, stat.CalculatePersentage);
            //}

            //tempFacade.Value.StartEmulate();
            //stat = new Statistics();
            //while (stat.CalculatePersentage < 1)
            //{
            //    Thread.Sleep(500);
            //    stat = tempFacade.Value.ReadStatistics();
            //    Debug.WriteLine("Read: {0}\tCalc: {1}", stat.ReadPercentage, stat.CalculatePersentage);
            //}

            var model = new HomeModel();
            await Task.WhenAll(
                Task.Run(() => model.Clusters = facade.Value.GetClusters(1, 0).Select(c => new ClusterModel(c)).ToList()),
                Task.Run(() => model.Users = facade.Value.GetUsers(1, 0).Select(u => new UserModel(u)).ToList()));

            return View(MVC.Home.Views.Index, model);
        }
    }
}
