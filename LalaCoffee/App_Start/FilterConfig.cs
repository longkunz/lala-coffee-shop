using System;
using System.Web;
using System.Web.Mvc;
using NLog;

namespace LalaCoffee
{
    public class AiExceptionFilter : HandleErrorAttribute
    {
        private static readonly ILogger Logger = LogManager.GetLogger("ErrorLogger");

        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                return;
            }

            if (filterContext.Exception != null)
            {
                var controllerName = (string)filterContext.RouteData?.Values?["controller"] ?? "Unknown";
                var actionName = (string)filterContext.RouteData?.Values?["action"] ?? "Unknown";
                Logger.Error(filterContext.Exception, "Unhandled MVC exception in {0}/{1}: {2}",
                    controllerName, actionName, filterContext.Exception.Message);
            }

            try
            {
                base.OnException(filterContext);
            }
            catch (NotImplementedException)
            {
                // Guard against unmocked HttpContextBase in test harnesses
            }
        }
    }

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AiExceptionFilter());
        }
    }
}
