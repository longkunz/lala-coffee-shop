using LalaCoffee.Controllers;
using LalaCoffee.Data;
using LalaCoffee.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

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
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();
			var formValues = formCollection.AllKeys.ToDictionary(k => k, k => formCollection[k] ?? string.Empty, StringComparer.OrdinalIgnoreCase);

			var name = formCollection["name"]?.Trim();
			var priceStr = formCollection["price"]?.Trim();
			var status = formCollection["status"] == "1" ? "1" : "0";
			var productIdStr = formCollection["product_id"];

			if (string.IsNullOrWhiteSpace(name))
			{
				fieldErrors["name"] = "Vui lòng nhập tên thuộc tính.";
			}
			decimal parsedPrice = 0;
			if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 0)
			{
				fieldErrors["price"] = "Giá thuộc tính phải từ 0đ trở lên.";
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
				return RedirectToAction("Index", "ManagePropertyProduct");
			}

			try
			{
				propertyProduct.Product = _productRepository.GetById(productId);
				propertyProduct.Name = name;
				propertyProduct.Price = parsedPrice;
				propertyProduct.Status = status;
				propertyProduct.CreatedAt = DateTime.Now;
				propertyProduct.UpdatedAt = DateTime.Now;
				Add(propertyProduct);
			}
			catch (Exception ex)
			{
				generalErrors.Add(ex.Message);
				TempData["Errors"] = generalErrors;
				TempData["FieldErrors"] = fieldErrors;
				TempData["FormValues"] = formValues;
				TempData["OpenModal"] = "create_modal";
				return RedirectToAction("Index", "ManagePropertyProduct");
			}

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
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();

			var idStr = formCollection["id"] ?? formCollection["Id"];
			var name = formCollection["name"]?.Trim();
			var priceStr = formCollection["price"]?.Trim();
			var status = formCollection["status"] == "1" ? "1" : "0";
			var productIdStr = formCollection["product_id"];

			int propertyProductId = 0;
			if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out propertyProductId))
			{
				TempData["Errors"] = new List<string> { "ID thuộc tính không hợp lệ." };
				return RedirectToAction("Index", "ManagePropertyProduct");
			}

			var propertyProduct = GetById(propertyProductId);
			if (propertyProduct == null)
			{
				TempData["Errors"] = new List<string> { "Không tìm thấy thuộc tính sản phẩm cần chỉnh sửa." };
				return RedirectToAction("Index", "ManagePropertyProduct");
			}

			propertyProduct.Name = name;
			propertyProduct.Status = status;

			int productId = 0;
			if (int.TryParse(productIdStr, out productId))
			{
				propertyProduct.Product = _productRepository.GetById(productId);
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				fieldErrors["name"] = "Vui lòng nhập tên thuộc tính.";
			}
			decimal parsedPrice = 0;
			if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 0)
			{
				fieldErrors["price"] = "Giá thuộc tính phải từ 0đ trở lên.";
			}
			else
			{
				propertyProduct.Price = parsedPrice;
			}
			if (propertyProduct.Product == null)
			{
				fieldErrors["product_id"] = "Vui lòng chọn sản phẩm hợp lệ.";
			}

			if (fieldErrors.Count > 0)
			{
				ViewBag.PriceInput = priceStr;
				ViewBag.Errors = fieldErrors.Values.ToList();
				ViewBag.FieldErrors = fieldErrors;
				ViewBag.Product = _productRepository.GetAll().ToList();
				return View(propertyProduct);
			}

			try
			{
				propertyProduct.UpdatedAt = DateTime.Now;
				Update(propertyProduct);
			}
			catch (Exception ex)
			{
				generalErrors.Add(ex.Message);
				ViewBag.PriceInput = priceStr;
				ViewBag.Errors = generalErrors;
				ViewBag.FieldErrors = fieldErrors;
				ViewBag.Product = _productRepository.GetAll().ToList();
				return View(propertyProduct);
			}

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
