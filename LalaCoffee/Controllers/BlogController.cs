using LalaCoffee.Models;
using System.Linq;
using System.Web.Mvc;

namespace LalaCoffee.Controllers
{
	public class BlogController : BaseController<Blog>
	{
		public ActionResult Index()
		{
			var b = GetAll().ToList();
			return View(b);
		}

		public ActionResult Readmore(int? id)
		{
			if (id == null)
			{
				return RedirectToAction("Index", "Blog");
			}
			var blog = GetById(id.Value);
			if (blog == null)
			{
				return RedirectToAction("Index", "Blog");
			}
			return View(blog);
		}
	}
}