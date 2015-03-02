using System.Web;
using System.Web.Optimization;

namespace WebApp
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include(
                "~/Scripts/jquery-{version}.js"
                , "~/Scripts/jquery-migrate-1.1.1.min.js"
                , "~/Scripts/jquery.form.js"));

            bundles.Add(new ScriptBundle("~/bundles/common")
                .Include(
                "~/Scripts/Common.js"
                , "~/Scripts/calendar.js"));

            bundles.Add(new ScriptBundle("~/bundles/calendar")
                .Include(
                "~/Scripts/jquery-ui.js",
                "~/Scripts/calendar.js"));

            bundles.Add(new ScriptBundle("~/bundles/admin")
                .Include("~/Scripts/Admin.js"));

            bundles.Add(new ScriptBundle("~/bundles/validation")
                .Include(
                 "~/Scripts/jquery.validate.min.js"
                 , "~/Scripts/jquery.validate.unobtrusive.min.js"
                 , "~/Scripts/MvcFoolproofJQueryValidation.min.js"
                 , "~/Scripts/mvcfoolproof.unobtrusive.min.js"));

            bundles.Add(new StyleBundle("~/Common").Include("~/Styles/common.css"));
            bundles.Add(new StyleBundle("~/Common").Include("~/Styles/jquery-ui.css"));
        }
    }
}