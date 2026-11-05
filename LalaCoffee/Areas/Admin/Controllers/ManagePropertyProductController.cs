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
    public class ManagePropertyProductController : BaseController<PropertyProduct>
    {
        private readonly IRepository<Product> _productRepository;

        public ManagePropertyProductController()
        {
            _productRepository = new Repository<Product>(Context);
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

        // GET: Admin/ManagePropertyProduct
        public ActionResult Index()
        {
            var product = _productRepository.GetAll().ToList();
            ViewBag.Product = product;
            var listPropertyProduct = GetAll().ToList();
            return View(listPropertyProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection formCollection, PropertyProduct propertyProduct)
        {
            List<string> errors = new List<string>();
            try
            {
                var name = formCollection["name"];
                var priceStr = formCollection["price"];
                var status = formCollection["status"] == "1" ? "1" : "0";
                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Chưa nhập tên thuộc tính");
                }
                decimal parsedPrice = 0;
                if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 0)
                {
                    errors.Add("Nhập giá ít nhất là 0đ");
                }
                if (errors.Count == 0)
                {
                    int productId = 0;
                    if (int.TryParse(formCollection["product_id"], out productId))
                    {
                        propertyProduct.Product = _productRepository.GetById(productId);
                    }
                    propertyProduct.Price = parsedPrice;
                    propertyProduct.Status = status;
                    Add(propertyProduct);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;

            return RedirectToAction("Index", "ManagePropertyProduct");
        }

        public ActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "ManagePropertyProduct");
            }
            var checkId = GetById(Id.Value);
            if (checkId == null)
            {
                return RedirectToAction("Index", "ManagePropertyProduct");
            }
            var product = _productRepository.GetAll().ToList();
            ViewBag.Product = product;
            PropertyProduct propertyProduct = GetById(Id.Value);
            return View(propertyProduct);
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
                var priceStr = formCollection["price"];
                var status = formCollection["status"] == "1" ? "1" : "0";

                int propertyProductId = 0;
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out propertyProductId))
                {
                    errors.Add("ID thuộc tính không hợp lệ");
                }

                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Chưa nhập tên thuộc tính");
                }
                decimal parsedPrice = 0;
                if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 0)
                {
                    errors.Add("Nhập giá ít nhất là 0đ");
                }
                if (errors.Count == 0)
                {
                    var propertyProduct = GetById(propertyProductId);
                    if (propertyProduct == null)
                    {
                        errors.Add("Không tìm thấy thuộc tính sản phẩm");
                    }
                    else
                    {
                        int productId = 0;
                        if (int.TryParse(formCollection["product_id"], out productId))
                        {
                            propertyProduct.Product = _productRepository.GetById(productId);
                        }
                        propertyProduct.Name = name;
                        propertyProduct.Price = parsedPrice;
                        propertyProduct.Status = status;
                        propertyProduct.UpdatedAt = DateTime.Now;
                        Update(propertyProduct);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;
            return RedirectToAction("Index", "ManagePropertyProduct");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }
            PropertyProduct propertyProduct = GetById(Id.Value);
            if (propertyProduct == null)
            {
                return Json(new { success = false, message = "Thuộc tính sản phẩm không tồn tại" });
            }
            Remove(propertyProduct);
            return Json(new { success = true, message = "Xóa thuộc tính sản phẩm thành công" });
        }
    }
}
