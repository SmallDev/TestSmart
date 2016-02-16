using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebClient.Services;

namespace WebClient
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;

            routes.Add(new ServiceRoute("api/streaming", new DependencyHostFactory(), typeof(IStreamService)));

            routes.MapRoute(
                name: "Cluster",
                url: "cluster/{id}",
                defaults: new { controller = MVC.Cluster.Name, action = MVC.Cluster.ActionNames.Index }
                );

            routes.MapRoute(
                name: "User",
                url: "user/{id}",
                defaults: new { controller = MVC.User.Name, action = MVC.User.ActionNames.Index }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}