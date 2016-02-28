using System.Web.Optimization;

namespace WebClient
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/lib").Include(
                      "~/Scripts/jquery.min.js",
                      "~/Scripts/kendo/kendo.all.min.js",
                      "~/Scripts/knockout-3.3.0.js",
                      "~/Scripts/customClocks.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                      "~/Scripts/app/ControlPanelViewModel.js"));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                      "~/assets/bootstrap/bootstrap.css",
                      "~/assets/bootstrap/bootstrap-theme.css",
                      "~/assets/kendo/kendo.bootstrap.min.css",
                      "~/assets/kendo/kendo.common-bootstrap.min.css",
                      "~/assets/flipclock.css",
                      "~/assets/customClock.css",
                      "~/assets/site.css"));
        }
    }
}