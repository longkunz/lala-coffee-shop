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
    public class ManageOptionProductController : BaseController<OptionProduct>
    {
        private readonly IRepository<Product> _productRepository;

        public ManageOptionProductController()
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

        // GET: Admin/ManageOptionProduct
        public ActionResult Index()
        {
            var product = _productRepository.GetAll().ToList();
            ViewBag.Product = product;
            var listOptionProduct = GetAll().ToList();
            return View(listOptionProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection formCollection, OptionProduct optionProduct)
        {
            List<string> errors = new List<string>();
            try
            {
                var name = formCollection["name"];
                var priceStr = formCollection["price"];
                var status = formCollection["status"] == "1" ? "1" : "0";
                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Chưa nhập tên tùy chọn");
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
                        optionProduct.Product = _productRepository.GetById(productId);
                    }
                    optionProduct.Price = parsedPrice;
                    optionProduct.Status = status;
                    Add(optionProduct);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;

            return RedirectToAction("Index", "ManageOptionProduct");
        }

        public ActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "ManageOptionProduct");
            }
            var checkId = GetById(Id.Value);
            if (checkId == null)
            {
                return RedirectToAction("Index", "ManageOptionProduct");
            }
            var product = _productRepository.GetAll().ToList();
            ViewBag.Product = product;
            OptionProduct optionProduct = GetById(Id.Value);
            return View(optionProduct);
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

                int optionProductId = 0;
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out optionProductId))
                {
                    errors.Add("ID tùy chọn không hợp lệ");
                }

                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Chưa nhập tên tùy chọn");
                }
                decimal parsedPrice = 0;
                if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 0)
                {
                    errors.Add("Nhập giá ít nhất là 0đ");
                }
                if (errors.Count == 0)
                {
                    var optionProduct = GetById(optionProductId);
                    if (optionProduct == null)
                    {
                        errors.Add("Không tìm thấy tùy chọn sản phẩm");
                    }
                    else
                    {
                        int productId = 0;
                        if (int.TryParse(formCollection["product_id"], out productId))
                        {
                            optionProduct.Product = _productRepository.GetById(productId);
                        }
                        optionProduct.Name = name;
                        optionProduct.Price = parsedPrice;
                        optionProduct.Status = status;
                        optionProduct.UpdatedAt = DateTime.Now;
                        Update(optionProduct);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;
            return RedirectToAction("Index", "ManageOptionProduct");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }
            OptionProduct optionProduct = GetById(Id.Value);
            if (optionProduct == null)
            {
                return Json(new { success = false, message = "Tùy chọn không tồn tại" });
            }
            Remove(optionProduct);
            return Json(new { success = true, message = "Xóa tùy chọn thành công" });
        }
    }
}
