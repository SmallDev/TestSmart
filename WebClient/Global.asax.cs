using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Logic.Dal;
using Logic.Dal.Wcf;
using Logic.Facades;

namespace WebClient
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            InitializeIoc();

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        private void InitializeIoc()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<WcfDataManager>().As<IDataManader>().SingleInstance();
            builder.RegisterType<Facade>().SingleInstance();

            builder.RegisterControllers(typeof(MvcApplication).Assembly).InstancePerDependency();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}