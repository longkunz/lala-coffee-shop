using LalaCoffee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LalaCoffee.Data;
using System.Data.Entity;

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

        public ActionResult Details(int Id)
        {
            Category category = GetById(Id);
            return View(category);
        }

        [Route("Collections/{Slug}")]
        public ActionResult Collection(string Slug)
        {
            var categories = GetAll();
            ViewBag.ListCate = categories;
            if (Slug == "All" || Slug == "")
            {
                var products = Context.Products.ToList();
                ViewBag.Categories = categories;
                return View(products);
            }
            else
            {
                var products = Context.Products.Where(x => x.Category.Slug == Slug).ToList();
                var categoriesBySlug = Context.Categories.Where(n => n.Slug == Slug).ToList();
                ViewBag.Categories = categoriesBySlug;
                return View(products);
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
