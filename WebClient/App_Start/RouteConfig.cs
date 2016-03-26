using System;
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

            routes.MapRoute("Clusters", "cluster", MVC.Cluster.GetList());
            routes.MapRoute("Cluster", "cluster/{id}", MVC.Cluster.Get(),
                constraints: new {id = @"\d+"});

            routes.MapRoute("KMeansClusters", "kmeans", MVC.KMeansCluster.GetList());
            routes.MapRoute("KMeansCluster", "kmeans/{set}/{id}", MVC.KMeansCluster.Get(),
                constraints: new { set = @"\d+", id = @"\d+" });

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new
                {
                    area = String.Empty,
                    controller = MVC.Home.Name,
                    action = MVC.Home.ActionNames.Index,
                    id = UrlParameter.Optional
                },
                new {controller = new NotEqual("api")});

            routes.Add("emulator", new ServiceRoute("api/emulator",
                new DependencyHostFactory(), typeof(IEmulatorService)));
        }
    }

    public class NotEqual : IRouteConstraint
    {
        private readonly String match = String.Empty;

        public NotEqual(String match)
        {
            this.match = match;
        }

        public Boolean Match(HttpContextBase httpContext, Route route, String parameterName,
            RouteValueDictionary values, RouteDirection routeDirection)
        {
            return String.Compare(values[parameterName].ToString(), match,
                StringComparison.InvariantCultureIgnoreCase) != 0;
        }
    }
}