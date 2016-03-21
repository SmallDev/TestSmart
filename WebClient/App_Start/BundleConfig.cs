using System.Web.Optimization;

namespace WebClient
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/lib").Include(
                      "~/Scripts/lib/jquery-{version}.js",
                      "~/Scripts/lib/kendo.all.min.js",
                      "~/Scripts/lib/knockout-{version}.js",
                      "~/Scripts/lib/js.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                      "~/Scripts/app/ControlPanelViewModel.js",
                      "~/Scripts/app/ClustersChartViewModel.js",
                      "~/Scripts/app/ClusterViewModel.js"));

            bundles.Add(new StyleBundle("~/Content/site/css").Include(
                      "~/Content/bootstrap/bootstrap.css",
                      "~/Content/bootstrap/bootstrap-theme.css",
                      "~/Content/kendo/kendo.bootstrap.min.css",
                      "~/Content/kendo/kendo.common-bootstrap.min.css",
                      "~/Content/site.css"));
        }
    }
}