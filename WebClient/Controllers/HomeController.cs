using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Logic.Facades;
using WebClient.Models;
using System.Net;

namespace WebClient.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly Lazy<EmulatorFacade> emulatorFacade;
        private readonly Lazy<LearningFacade> learningFacade;
        private readonly Lazy<StatisticsFacade> statisticsFacade;

        public HomeController(Func<EmulatorFacade> emulatorFacade, Func<LearningFacade> learningFacade, Func<StatisticsFacade> statisticsFacade)
        {
            this.emulatorFacade = new Lazy<EmulatorFacade>(emulatorFacade);
            this.learningFacade = new Lazy<LearningFacade>(learningFacade);
            this.statisticsFacade = new Lazy<StatisticsFacade>(statisticsFacade);
        }

        public virtual ActionResult Index()
        {
            var isStarted = emulatorFacade.Value.IsStarted() || learningFacade.Value.IsStarted();
            return View(MVC.Home.Views.Index, new { IsStarted = isStarted });
        }

        public virtual ActionResult Start()
        {
            Task.WhenAll(emulatorFacade.Value.StartRead(), learningFacade.Value.StartLearning());

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        public virtual async Task<ActionResult> Stop()
        {
            await Task.WhenAll(emulatorFacade.Value.StopRead(), learningFacade.Value.StopLearning());
            
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public virtual void SetReadVelocity(double velocity)
        {
            emulatorFacade.Value.SetVelocity(velocity);
        }

        [HttpPost]
        public virtual void SetLearningVelocity(double velocity)
        {
            learningFacade.Value.SetVelocity(velocity);
        }

        [HttpPost]
        public virtual void SetAllTime(string allTime)
        {
            emulatorFacade.Value.SetAllTime(TimeSpan.Parse(allTime));
        }

        [ChildActionOnly]
        public virtual ActionResult GetControlPanel()
        {  
            return View(MVC.Shared.Views.ControlPanel, GetControlModel().GetAwaiter().GetResult());
        }
        public virtual async Task<JsonResult> GetControlData()
        {
            return Json(await GetControlModel(), JsonRequestBehavior.AllowGet);
        }

        private async Task<ControlModel> GetControlModel()
        {
            var model = new ControlModel();

            await Task.WhenAll(
                Task.Run(() => model.ReadVelocity = emulatorFacade.Value.GetVelocity()),
                Task.Run(() => model.LearnVelocity = learningFacade.Value.GetVelocity()),
                Task.Run(() => model.SetAllTime(emulatorFacade.Value.GetAllTime())),
                Task.Run(() => model.SetReadTime(emulatorFacade.Value.GetReadTime())),
                Task.Run(() => model.SetCalcTime(learningFacade.Value.GetCalcTime())))
                .ConfigureAwait(false);

            return model;
        }

        [HttpPost]
        public virtual void InitClusters(InitClusterModel model)
        {
            statisticsFacade.Value.InitClusters(model.ClustersNumber);
            if (model.ClearData)
            {
                emulatorFacade.Value.Clear();
            }
        }
    }
}
