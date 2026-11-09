using NLog;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LalaCoffee
{
	public class MvcApplication : System.Web.HttpApplication
	{
		private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			UnityConfig.RegisterComponents();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			UnityDependencyResolver.DisposePerRequestContainer(HttpContext.Current);
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			var exception = Server.GetLastError();
			if (exception != null)
			{
				Logger.Error(exception, "Unhandled application error: {0}", exception.Message);
			}
		}

		public override void Dispose()
		{
			if (DependencyResolver.Current is IDisposable disposableResolver)
			{
				disposableResolver.Dispose();
			}
			base.Dispose();
		}
	}
}
