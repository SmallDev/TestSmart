using System;
using System.Web.Mvc;
using Logic.Facades;

namespace WebClient.Controllers
{
    public partial class UserController : Controller
    {
        private readonly Lazy<StatisticsFacade> statisticsFacade;
        public UserController(Func<StatisticsFacade> statisticsFacade)
        {
            this.statisticsFacade = new Lazy<StatisticsFacade>(statisticsFacade);
        }

        public virtual ActionResult GetList(String macFilter, Int32 page, Int32 size)
        {
            var users = statisticsFacade.Value.GetUsers(macFilter, page, size);
            return View(MVC.User.Views.Index, users);
        }

        public virtual ActionResult Get(Int32 id)
        {
            var user = statisticsFacade.Value.GetUser(id);
            return View(MVC.User.Views.Index, user);
        }
    }
}
