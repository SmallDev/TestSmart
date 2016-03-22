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

        public HomeController(Func<EmulatorFacade> emulatorFacade, Func<LearningFacade> learningFacade)
        {
            this.emulatorFacade = new Lazy<EmulatorFacade>(emulatorFacade);
            this.learningFacade = new Lazy<LearningFacade>(learningFacade);
        }

        public virtual ActionResult Index()
        {
            return View(MVC.Home.Views.Index, new { IsStarted= true});
        }

        public virtual async Task<ActionResult> Start()
        {
            await Task.WhenAll(emulatorFacade.Value.StartRead(), learningFacade.Value.StartLearning());
            

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        public virtual ActionResult Stop()
        {
            Task.WaitAll(emulatorFacade.Value.StopRead(), learningFacade.Value.StopLearning());

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
    }
}
