using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Logic.Facades;
using Logic.Model;

namespace WebClient.Controllers
{
    public partial class UserController : Controller
    {
        private readonly Lazy<StatisticsFacade> statisticsFacade;
        public UserController(Func<StatisticsFacade> statisticsFacade)
        {
            this.statisticsFacade = new Lazy<StatisticsFacade>(statisticsFacade);
        }
        public virtual ActionResult Index(Int32 id)
        {
            var user = statisticsFacade.Value.GetUser(id);
            return View(MVC.User.Views.Index, user);
        }

        private IList<User> FindUsers(String macFilter, Int32 page, Int32 size)
        {
            return statisticsFacade.Value.GetUsers(macFilter, page, size);
        }
    }
}
