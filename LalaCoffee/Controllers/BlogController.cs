using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LalaCoffee.Models;

namespace LalaCoffee.Controllers
{
    public class BlogController : BaseController<Blog>
    {
        // GET: Blog
        public ActionResult Index()
        {
            var b = GetAll().ToList();
            return View(b);
        }
        public ActionResult Readmore(int? id) {
            if (id == null) {
                return RedirectToAction("Index", "Blog");
            }
            var blog = GetById(id.Value);
            if (blog == null) {
                return RedirectToAction("Index", "Blog");
            }
            return View(blog);
        }
    }
}