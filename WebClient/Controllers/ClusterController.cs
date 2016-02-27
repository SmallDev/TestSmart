using System;
using System.Web.Mvc;

namespace WebClient.Controllers
{
    public partial class ClusterController : Controller
    {
        //
        // GET: /Cluster/

        public virtual ActionResult Index(Int32 id)
        {
            return View();
        }
    }
}
