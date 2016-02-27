using System;
using System.Web.Mvc;

namespace WebClient.Controllers
{
    public partial class UserController : Controller
    {
        //
        // GET: /User/

        public virtual ActionResult Index(Int32 id)
        {
            return View();
        }

    }
}
