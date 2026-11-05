using System;
using System.Web.Mvc;

namespace LalaCoffee.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        [AllowAnonymous]
        public ActionResult Index()
        {
            try
            {
                if (Response != null)
                {
                    Response.StatusCode = 500;
                    Response.TrySkipIisCustomErrors = true;
                }
            }
            catch
            {
                // Ignore environment / mock differences
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        // GET: Error/NotFound
        [AllowAnonymous]
        public ActionResult NotFound()
        {
            try
            {
                if (Response != null)
                {
                    Response.StatusCode = 404;
                    Response.TrySkipIisCustomErrors = true;
                }
            }
            catch
            {
                // Ignore environment / mock differences
            }
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
