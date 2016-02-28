using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Logic.Facades;
using WebClient.Models;

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
            return View(MVC.Home.Views.Index);
        }

        public virtual JsonResult Start()
        {
            return Json(new Object());
        }
        public virtual JsonResult Stop()
        {
            return Json(new Object());
        }

        [ChildActionOnly]
        public virtual ActionResult GetControlPanel()
        {
            return View(MVC.Shared.Views.ControlPanel, GetControlModel());
        }
        public virtual JsonResult GetControlData()
        {
            return Json(GetControlModel());
        }

        private ControlModel GetControlModel()
        {
            var model = new ControlModel();

            Task.WaitAll(
                Task.Run(() => model.Velocity = emulatorFacade.Value.GetVelocity()),
                Task.Run(() => model.SetAllTime(emulatorFacade.Value.GetAllTime())),
                Task.Run(() => model.SetReadTime(emulatorFacade.Value.GetReadTime())),
                Task.Run(() => model.SetCalcTime(learningFacade.Value.GetCalcTime())));

            return model;
        }
    }
}
