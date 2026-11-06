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
            var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var generalErrors = new List<string>();
            var formValues = formCollection.AllKeys.ToDictionary(k => k, k => formCollection[k] ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            var name = formCollection["name"]?.Trim();
            var priceStr = formCollection["price"]?.Trim();
            var status = formCollection["status"] == "1" ? "1" : "0";
            var productIdStr = formCollection["product_id"];

            if (string.IsNullOrWhiteSpace(name))
            {
                fieldErrors["name"] = "Vui lòng nhập tên tùy chọn.";
            }
            decimal parsedPrice = 0;
            if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 0)
            {
                fieldErrors["price"] = "Giá tùy chọn phải từ 0đ trở lên.";
            }
            int productId = 0;
            if (!int.TryParse(productIdStr, out productId) || _productRepository.GetById(productId) == null)
            {
                fieldErrors["product_id"] = "Vui lòng chọn sản phẩm hợp lệ.";
            }

            if (fieldErrors.Count > 0)
            {
                TempData["Errors"] = fieldErrors.Values.ToList();
                TempData["FieldErrors"] = fieldErrors;
                TempData["FormValues"] = formValues;
                TempData["OpenModal"] = "create_modal";
                return RedirectToAction("Index", "ManageOptionProduct");
            }

            try
            {
                optionProduct.Product = _productRepository.GetById(productId);
                optionProduct.Name = name;
                optionProduct.Price = parsedPrice;
                optionProduct.Status = status;
                optionProduct.CreatedAt = DateTime.Now;
                optionProduct.UpdatedAt = DateTime.Now;
                Add(optionProduct);
            }
            catch (Exception ex)
            {
                generalErrors.Add(ex.Message);
                TempData["Errors"] = generalErrors;
                TempData["FieldErrors"] = fieldErrors;
                TempData["FormValues"] = formValues;
                TempData["OpenModal"] = "create_modal";
                return RedirectToAction("Index", "ManageOptionProduct");
            }

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
            var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var generalErrors = new List<string>();

            var idStr = formCollection["id"] ?? formCollection["Id"];
            var name = formCollection["name"]?.Trim();
            var priceStr = formCollection["price"]?.Trim();
            var status = formCollection["status"] == "1" ? "1" : "0";
            var productIdStr = formCollection["product_id"];

            int optionProductId = 0;
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out optionProductId))
            {
                TempData["Errors"] = new List<string> { "ID tùy chọn không hợp lệ." };
                return RedirectToAction("Index", "ManageOptionProduct");
            }

            var optionProduct = GetById(optionProductId);
            if (optionProduct == null)
            {
                TempData["Errors"] = new List<string> { "Không tìm thấy tùy chọn sản phẩm cần chỉnh sửa." };
                return RedirectToAction("Index", "ManageOptionProduct");
            }

            optionProduct.Name = name;
            optionProduct.Status = status;

            int productId = 0;
            if (int.TryParse(productIdStr, out productId))
            {
                optionProduct.Product = _productRepository.GetById(productId);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                fieldErrors["name"] = "Vui lòng nhập tên tùy chọn.";
            }
            decimal parsedPrice = 0;
            if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 0)
            {
                fieldErrors["price"] = "Giá tùy chọn phải từ 0đ trở lên.";
            }
            else
            {
                optionProduct.Price = parsedPrice;
            }
            if (optionProduct.Product == null)
            {
                fieldErrors["product_id"] = "Vui lòng chọn sản phẩm hợp lệ.";
            }

            if (fieldErrors.Count > 0)
            {
                ViewBag.PriceInput = priceStr;
                ViewBag.Errors = fieldErrors.Values.ToList();
                ViewBag.FieldErrors = fieldErrors;
                ViewBag.Product = _productRepository.GetAll().ToList();
                return View(optionProduct);
            }

            try
            {
                optionProduct.UpdatedAt = DateTime.Now;
                Update(optionProduct);
            }
            catch (Exception ex)
            {
                generalErrors.Add(ex.Message);
                ViewBag.PriceInput = priceStr;
                ViewBag.Errors = generalErrors;
                ViewBag.FieldErrors = fieldErrors;
                ViewBag.Product = _productRepository.GetAll().ToList();
                return View(optionProduct);
            }

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
