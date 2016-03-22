using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Common.Logging;
using Logic.Dal;
using Logic.Dal.NHibernate;
using Logic.Facades;
using WebClient.Services;

namespace WebClient
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            InitializeIoc();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void InitializeIoc()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<NHibernateDataManagerFactory>().As<IDataManagerFactrory>().SingleInstance();
            builder.RegisterType<ConfigService>().As<IDbConfig>().SingleInstance();
            
            builder.RegisterType<EmulatorFacade>().SingleInstance();
            builder.RegisterType<LearningFacade>().SingleInstance();
            builder.RegisterType<StatisticsFacade>().SingleInstance();
            builder.RegisterType<ConfigService>().As<IEmulatorConfig>().SingleInstance();

            builder.RegisterInstance(LogManager.Adapter).SingleInstance();
            builder.RegisterControllers(typeof(MvcApplication).Assembly).InstancePerDependency();
            builder.RegisterType<EmulatorService>().As<IEmulatorService>().SingleInstance();
            
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}