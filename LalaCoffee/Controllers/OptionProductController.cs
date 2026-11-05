using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LalaCoffee.Data;
using LalaCoffee.Models;

namespace LalaCoffee.Controllers
{
    public class OptionProductController : BaseController<OptionProduct>
    {
        private readonly IRepository<Product> _productRepository;

        public OptionProductController()
        {
            _productRepository = new Repository<Product>(Context);
        }

        // GET: OptionProduct
        public ActionResult Index()
        {
            var OptionProducts = GetAll().ToList();
            var Products = _productRepository.GetAll().ToList();
            ViewBag.Products = Products;
            return View(OptionProducts);
        }
    }
}
