using System.Web.Mvc;

namespace LalaCoffee.Areas.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class StatsController : Controller
	{
		// GET: Admin/Stats
		public ActionResult Index()
		{
			return RedirectToAction("Index", "ManageOrder");
		}
	}
}
