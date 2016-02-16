using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
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

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        private void InitializeIoc()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<NHibernateDataManagerFactory>().As<IDataManagerFactrory>().SingleInstance();
            builder.RegisterType<ConfigService>().As<IDbConfig>().SingleInstance();
            
            builder.RegisterType<DataFacade>().SingleInstance();
            builder.RegisterType<AlgorithmFacade>().SingleInstance();
            builder.RegisterType<ConfigService>().As<IReaderConfig>().SingleInstance();

            builder.RegisterControllers(typeof(MvcApplication).Assembly).InstancePerDependency();
            builder.RegisterType<StreamService>().As<IStreamService>().SingleInstance();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}