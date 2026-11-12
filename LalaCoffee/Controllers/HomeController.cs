using LalaCoffee.Data;
using LalaCoffee.Models;
using System.Linq;
using System.Web.Mvc;

namespace LalaCoffee.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<Product> _productRepository;
		public HomeController()
		{
			_context = new ApplicationDbContext();
			_categoryRepository = new Repository<Category>(_context);
			_productRepository = new Repository<Product>(_context);
		}

		public ActionResult Index()
		{
			var showProducts = _context.Products.Where(c => c.Show == 1).ToList();
			ViewBag.showProducts = showProducts;

			ViewBag.Categories = _categoryRepository.GetAll().ToList();

			ViewBag.FeaturedProducts = _context.Products
				.Where(c => c.Show == 1)
				.OrderBy(c => c.Id)
				.Take(6)
				.ToList();

			var blogs = _context.Blogs
				.Where(b => b.Published)
				.OrderByDescending(b => b.CreatedDate)
				.Take(6)
				.ToList();

			if (!blogs.Any())
			{
				blogs = _context.Blogs
					.OrderByDescending(b => b.CreatedDate)
					.Take(6)
					.ToList();
			}
			ViewBag.Blogs = blogs;

			var products = _productRepository.GetAll().ToList().Take(10);
			return View(products);
		}
		public ActionResult Collection(string id)
		{
			var category = _context.Categories.FirstOrDefault(c => c.Slug == id);
			var products = _context.Products.Where(c => c.Category.Id == category.Id).ToList();
			ViewBag.Products = products;
			ViewBag.Category = category;
			return View();
		}

		public ActionResult Menu()
		{
			var category = _categoryRepository.GetAll();
			return View(category);
		}
		public ActionResult Contact()
		{
			return View(new ContactViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Contact(ContactViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ với LalaCoffee! Chúng tôi đã nhận được lời nhắn và sẽ phản hồi qua email/số điện thoại trong thời gian sớm nhất.";
			return RedirectToAction("Contact");
		}
		public ActionResult About()
		{
			return View();
		}
		public ActionResult KhuyenMai()
		{
			return View();
		}
	}
}