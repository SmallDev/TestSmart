using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Logic.Facades;
using WebClient.Models;

namespace WebClient.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly Lazy<EmulatorFacade> tempFacade;
        private readonly Lazy<DataFacade> facade;

        public HomeController(Func<DataFacade> facade, Func<EmulatorFacade> tempFacade)
        {
            this.tempFacade = new Lazy<EmulatorFacade>(tempFacade);
            this.facade = new Lazy<DataFacade>(facade);
        }

        public async virtual Task<ActionResult> Index()
        {
            tempFacade.Value.StartRead(new CancellationTokenSource());

            var model = new HomeModel();
            await Task.WhenAll(
                Task.Run(() => model.Clusters = facade.Value.GetClusters(1, 0).Select(c => new ClusterModel(c)).ToList()),
                Task.Run(() => model.Users = facade.Value.GetUsers(1, 0).Select(u => new UserModel(u)).ToList()));

            return View(MVC.Home.Views.Index, model);
        }
    }
}
