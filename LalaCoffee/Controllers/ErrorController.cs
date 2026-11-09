using System.Web.Mvc;

namespace LalaCoffee.Controllers
{
	public class ErrorController : Controller
	{
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
			}
			return View("~/Views/Shared/Error.cshtml");
		}

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
			}
			return View("~/Views/Shared/Error.cshtml");
		}
	}
}
