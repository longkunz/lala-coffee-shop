using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using LalaCoffee.Controllers;
using LalaCoffee.Data;
using LalaCoffee.Models;

namespace LalaCoffee.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageProductController : BaseController<Product>
    {
        private readonly IRepository<Category> _categoryRepository;

        public ManageProductController()
        {
            _categoryRepository = new Repository<Category>(Context);
        }

        private static bool TryParsePrice(string priceInput, out decimal price)
        {
            price = 0;
            if (string.IsNullOrWhiteSpace(priceInput))
            {
                return false;
            }

            var cleanPrice = priceInput.Trim();
            if (decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out price) ||
                decimal.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.CurrentCulture, out price))
            {
                return true;
            }

            var normalized = cleanPrice.Replace(",", "").Replace(".", "");
            return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out price);
        }

        // GET: Admin/ManageProduct
        public ActionResult Index()
        {
            var categories = _categoryRepository.GetAll().ToList();
            ViewBag.Categories = categories;
            var listProduct = GetAll().ToList();
            return View(listProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection formCollection, Product product)
        {
            List<string> errors = new List<string>();
            try
            {
                var name = formCollection["name"];
                var slug = formCollection["slug"];
                var priceStr = formCollection["price"];
                var description = formCollection["description"];
                var image = formCollection["image"];
                var status = formCollection["status"] == "1" ? "1" : "0";
                var checkSlug = Context.Products.Count(x => x.Slug == slug);
                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Chưa nhập tên sản phẩm");
                }
                if (checkSlug > 0)
                {
                    errors.Add("Slug đã tồn tại");
                }
                if (string.IsNullOrEmpty(description))
                {
                    errors.Add("Chưa nhập mô tả sản phẩm");
                }
                decimal parsedPrice = 0;
                if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 1000)
                {
                    errors.Add("Nhập giá ít nhất là 1000đ");
                }
                if (errors.Count == 0)
                {
                    int categoryId = 0;
                    if (int.TryParse(formCollection["category_id"], out categoryId))
                    {
                        product.Category = _categoryRepository.GetById(categoryId);
                    }
                    product.Price = parsedPrice;
                    product.Status = status;
                    Add(product);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;
            
            return RedirectToAction("Index", "ManageProduct");
        }

        public ActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "ManageProduct");
            }
            var checkId = GetById(Id.Value);
            if (checkId == null)
            {
                return RedirectToAction("Index", "ManageProduct");
            }
            var categories = _categoryRepository.GetAll().ToList();
            ViewBag.Categories = categories;
            Product product = GetById(Id.Value);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection formCollection)
        {
            List<string> errors = new List<string>();
            try
            {
                var idStr = formCollection["id"] ?? formCollection["Id"];
                var name = formCollection["name"];
                var slug = formCollection["slug"];
                var priceStr = formCollection["price"];
                var description = formCollection["description"];
                var image = formCollection["image"];
                var status = formCollection["status"] == "1" ? "1" : "0";

                int productId = 0;
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out productId))
                {
                    errors.Add("ID sản phẩm không hợp lệ");
                }

                var checkSlug = Context.Products.Count(x => x.Slug == slug);
                var getProductContainsSlug = Context.Products.FirstOrDefault(x => x.Slug == slug);
                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Chưa nhập tên sản phẩm");
                }
                if (checkSlug > 0 && getProductContainsSlug != null && getProductContainsSlug.Id != productId)
                {
                    errors.Add("Slug đã tồn tại");
                }
                if (string.IsNullOrEmpty(description))
                {
                    errors.Add("Chưa nhập mô tả sản phẩm");
                }
                decimal parsedPrice = 0;
                if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 1000)
                {
                    errors.Add("Nhập giá ít nhất là 1000đ");
                }
                if (errors.Count == 0)
                {
                    var product = GetById(productId);
                    if (product == null)
                    {
                        errors.Add("Không tìm thấy sản phẩm");
                    }
                    else
                    {
                        product.Name = name;
                        product.Slug = slug;
                        product.Price = parsedPrice;
                        product.Description = description;
                        product.Image = image;
                        product.Status = status;
                        product.UpdatedAt = DateTime.Now;

                        int categoryId = 0;
                        if (int.TryParse(formCollection["category_id"], out categoryId))
                        {
                            product.Category = _categoryRepository.GetById(categoryId);
                        }
                        Update(product);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;
            return RedirectToAction("Index", "ManageProduct");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }
            Product product = GetById(Id.Value);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }
            Remove(product);
            return Json(new { success = true, message = "Xóa sản phẩm thành công" });
        }
    }
}
