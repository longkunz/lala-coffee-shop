using System.Web.Optimization;

namespace LalaCoffee
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.validate*"));

			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
					  "~/Content/lib/bootstrap/dist/js/bootstrap.bundle.min.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
					  "~/Content/lib/bootstrap/dist/css/bootstrap.min.css",
					  "~/Content/site.css"));
		}
	}
}
