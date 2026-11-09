using LalaCoffee.Data;
using LalaCoffee.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace LalaCoffee.Controllers
{
	public class CategoryController : BaseController<Category>
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<OptionProduct> _optionProductRepository;

		public CategoryController()
		{
			_productRepository = new Repository<Product>(Context);
			_optionProductRepository = new Repository<OptionProduct>(Context);
		}

		public ActionResult Index()
		{
			var listCategory = GetAll().ToList();
			return View(listCategory);
		}

		[Route("Category/Details/{id?}")]
		public ActionResult Details(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return RedirectToAction("Collection", new { Slug = "All" });
			}

			string slug = id;
			if (int.TryParse(id, out int numericId))
			{
				var category = Context.Categories.FirstOrDefault(c => c.Id == numericId);
				if (category != null && !string.IsNullOrEmpty(category.Slug))
				{
					slug = category.Slug;
				}
			}

			return Collection(slug);
		}

		[Route("Collections/{Slug?}")]
		public ActionResult Collection(string Slug, string search = null)
		{
			var categories = GetAll().ToList();
			ViewBag.ListCate = categories;
			ViewBag.CurrentSlug = Slug;
			ViewBag.Search = search;

			var productQuery = Context.Products.Include(p => p.Category).AsNoTracking().AsQueryable();

			if (!string.IsNullOrEmpty(Slug) && !string.Equals(Slug, "All", StringComparison.OrdinalIgnoreCase))
			{
				productQuery = productQuery.Where(x => x.Category.Slug == Slug);
			}

			if (!string.IsNullOrWhiteSpace(search))
			{
				var s = search.Trim();
				productQuery = productQuery.Where(x => (x.Name != null && x.Name.Contains(s)) || (x.Description != null && x.Description.Contains(s)));
			}

			var products = productQuery.ToList();

			if (string.IsNullOrEmpty(Slug) || string.Equals(Slug, "All", StringComparison.OrdinalIgnoreCase))
			{
				ViewBag.Categories = categories;
				return View("Collection", products);
			}
			else
			{
				var categoriesBySlug = Context.Categories.Where(n => n.Slug == Slug).ToList();
				if (!categoriesBySlug.Any())
				{
					categoriesBySlug = categories;
				}
				ViewBag.Categories = categoriesBySlug;
				return View("Collection", products);
			}
		}

		public ActionResult DetailProduct(int Id)
		{
			var Product = _productRepository.GetById(Id);
			var c = _optionProductRepository.GetAll().ToList();
			c = Context.OptionProducts.Where(p => p.Product.Id == Id).ToList();
			ViewBag.optionProducts = c;
			return View(Product);
		}
	}
}
