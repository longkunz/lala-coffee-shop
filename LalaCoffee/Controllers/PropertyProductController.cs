using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LalaCoffee.Data;
using LalaCoffee.Models;

namespace LalaCoffee.Controllers
{
    public class PropertyProductController : BaseController<PropertyProduct>
    {
        private readonly IRepository<Product> _productRepository;

        public PropertyProductController()
        {
            _productRepository = new Repository<Product>(Context);
        }

        // GET: PropertyProduct
        public ActionResult Index()
        {
            var PropertyProducts = GetAll().ToList();
            var Products = _productRepository.GetAll().ToList();
            ViewBag.Products = Products;
            return View(PropertyProducts);
        }
    }
}
