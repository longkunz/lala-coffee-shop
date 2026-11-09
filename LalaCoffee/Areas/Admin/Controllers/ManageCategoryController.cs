using LalaCoffee.Controllers;
using LalaCoffee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LalaCoffee.Areas.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class ManageCategoryController : BaseController<Category>
	{
		public ManageCategoryController()
		{

		}
		// GET: Admin/ManageCategory
		public ActionResult Index()
		{
			var listCategory = GetAll().ToList();
			return View(listCategory);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(FormCollection formCollection, Category category)
		{
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();
			var formValues = formCollection.AllKeys.ToDictionary(k => k, k => formCollection[k] ?? string.Empty, StringComparer.OrdinalIgnoreCase);

			var name = formCollection["name"]?.Trim();
			var slug = formCollection["slug"]?.Trim();
			var description = formCollection["description"]?.Trim();
			var parentidStr = formCollection["parentid"];
			var status = formCollection["status"] == "1" ? "1" : "0";

			if (string.IsNullOrWhiteSpace(name))
			{
				fieldErrors["name"] = "Vui lòng nhập tên danh mục.";
			}
			if (string.IsNullOrWhiteSpace(slug))
			{
				fieldErrors["slug"] = "Vui lòng nhập slug danh mục.";
			}
			else if (Context.Categories.Any(x => x.Slug == slug))
			{
				fieldErrors["slug"] = "Slug đã tồn tại, vui lòng chọn slug khác.";
			}
			if (string.IsNullOrWhiteSpace(description))
			{
				fieldErrors["description"] = "Vui lòng nhập mô tả danh mục.";
			}

			if (fieldErrors.Count > 0)
			{
				TempData["Errors"] = fieldErrors.Values.ToList();
				TempData["FieldErrors"] = fieldErrors;
				TempData["FormValues"] = formValues;
				TempData["OpenModal"] = "create_modal";
				return RedirectToAction("Index", "ManageCategory");
			}

			try
			{
				int parentId = 0;
				if (int.TryParse(parentidStr, out parentId))
				{
					category.ParentId = parentId;
				}
				category.Name = name;
				category.Slug = slug;
				category.Description = description;
				category.Status = status;
				category.CreatedAt = DateTime.Now;
				category.UpdatedAt = DateTime.Now;
				Add(category);
			}
			catch (Exception ex)
			{
				generalErrors.Add(ex.Message);
				TempData["Errors"] = generalErrors;
				TempData["FieldErrors"] = fieldErrors;
				TempData["FormValues"] = formValues;
				TempData["OpenModal"] = "create_modal";
				return RedirectToAction("Index", "ManageCategory");
			}

			return RedirectToAction("Index", "ManageCategory");
		}
		public ActionResult Edit(int? Id)
		{
			if (Id == null)
			{
				return RedirectToAction("Index", "ManageCategory");
			}
			var checkId = GetById(Id.Value);
			if (checkId == null)
			{
				return RedirectToAction("Index", "ManageCategory");
			}
			Category category = GetById(Id.Value);
			ViewBag.Categories = GetAll().ToList();
			return View(category);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(FormCollection formCollection)
		{
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();

			var idStr = formCollection["id"] ?? formCollection["Id"];
			var name = formCollection["name"]?.Trim();
			var slug = formCollection["slug"]?.Trim();
			var description = formCollection["description"]?.Trim();
			var status = formCollection["status"] == "1" ? "1" : "0";
			var parent_idStr = formCollection["parent_id"] ?? formCollection["parentid"];

			int categoryId = 0;
			if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out categoryId))
			{
				TempData["Errors"] = new List<string> { "ID danh mục không hợp lệ." };
				return RedirectToAction("Index", "ManageCategory");
			}

			var category = GetById(categoryId);
			if (category == null)
			{
				TempData["Errors"] = new List<string> { "Không tìm thấy danh mục cần chỉnh sửa." };
				return RedirectToAction("Index", "ManageCategory");
			}

			// Preserve entered values on model
			category.Name = name;
			category.Slug = slug;
			category.Description = description;
			category.Status = status;
			int parentId = 0;
			if (int.TryParse(parent_idStr, out parentId))
			{
				category.ParentId = parentId;
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				fieldErrors["name"] = "Vui lòng nhập tên danh mục.";
			}
			if (string.IsNullOrWhiteSpace(slug))
			{
				fieldErrors["slug"] = "Vui lòng nhập slug danh mục.";
			}
			else
			{
				var slugExists = Context.Categories.Any(x => x.Slug == slug && x.Id != categoryId);
				if (slugExists)
				{
					fieldErrors["slug"] = "Slug đã tồn tại, vui lòng chọn slug khác.";
				}
			}
			if (string.IsNullOrWhiteSpace(description))
			{
				fieldErrors["description"] = "Vui lòng nhập mô tả danh mục.";
			}

			if (fieldErrors.Count > 0)
			{
				ViewBag.Errors = fieldErrors.Values.ToList();
				ViewBag.FieldErrors = fieldErrors;
				ViewBag.Categories = GetAll().ToList();
				return View(category);
			}

			try
			{
				category.UpdatedAt = DateTime.Now;
				Update(category);
			}
			catch (Exception ex)
			{
				generalErrors.Add(ex.Message);
				ViewBag.Errors = generalErrors;
				ViewBag.FieldErrors = fieldErrors;
				ViewBag.Categories = GetAll().ToList();
				return View(category);
			}

			return RedirectToAction("Index", "ManageCategory");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int? Id)
		{
			if (Id == null)
			{
				return Json(new { success = false, message = "ID không hợp lệ" });
			}
			Category category = GetById(Id.Value);
			if (category == null)
			{
				return Json(new { success = false, message = "Danh mục không tồn tại" });
			}
			return TryRemove(category, "Xóa danh mục thành công", "Không thể xóa danh mục này vì vẫn còn sản phẩm thuộc danh mục. Hãy chuyển hoặc xóa các sản phẩm đó trước.");
		}
	}
}
