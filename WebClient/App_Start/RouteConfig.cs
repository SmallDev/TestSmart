using System.ServiceModel.Activation;
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

            routes.MapRoute("Clusters", "cluster", MVC.Cluster.GetList());
            routes.MapRoute("Cluster", "cluster/{id}", MVC.Cluster.Get());

            routes.MapRoute("Users", "user", MVC.User.GetList());
            routes.MapRoute("User", "user/{id}", MVC.User.Get());

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = MVC.Home.Name, action = MVC.Home.ActionNames.Index, id = UrlParameter.Optional }
            );

            routes.Add("emulator", new ServiceRoute("api/emulator",
                new DependencyHostFactory(), typeof(IEmulatorService)));
        }
    }
}