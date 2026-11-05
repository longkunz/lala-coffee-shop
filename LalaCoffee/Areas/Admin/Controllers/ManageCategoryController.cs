using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            List<string> errors = new List<string>();
            try
            {
                var name = formCollection["name"];
                var slug = formCollection["slug"];
                var description = formCollection["description"];
                var parentid = formCollection["parentid"];
                var status = formCollection["status"] == "1" ? "1" : "0";
                var checkSlug = Context.Categories.Count(x => x.Slug == slug);
                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Chưa nhập tên danh mục");
                }
                if (checkSlug > 0)
                {
                    errors.Add("Slug đã tồn tại");
                }
                if (string.IsNullOrEmpty(description))
                {
                    errors.Add("Chưa nhập mô tả danh mục");
                }
                if (errors.Count == 0)
                {
                    category.Status = status;
                    Add(category);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;

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
            List<string> errors = new List<string>();
            try
            {
                var idStr = formCollection["id"] ?? formCollection["Id"];
                var name = formCollection["name"];
                var slug = formCollection["slug"];
                var description = formCollection["description"];
                var status = formCollection["status"] == "1" ? "1" : "0";

                int categoryId = 0;
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out categoryId))
                {
                    errors.Add("ID danh mục không hợp lệ");
                }

                var checkSlug = Context.Categories.Count(x => x.Slug == slug);
                var getCategoryContainsSlug = Context.Categories.FirstOrDefault(x => x.Slug == slug);
                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Chưa nhập tên danh mục");
                }
                if (checkSlug > 0 && getCategoryContainsSlug != null && getCategoryContainsSlug.Id != categoryId)
                {
                    errors.Add("Slug đã tồn tại");
                }
                if (string.IsNullOrEmpty(description))
                {
                    errors.Add("Chưa nhập mô tả danh mục");
                }
                if (errors.Count == 0)
                {
                    var category = GetById(categoryId);
                    if (category == null)
                    {
                        errors.Add("Không tìm thấy danh mục");
                    }
                    else
                    {
                        category.Name = name;
                        category.Slug = slug;
                        category.Status = status;
                        category.Description = description;
                        category.UpdatedAt = DateTime.Now;
                        Update(category);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;
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
            Remove(category);
            return Json(new { success = true, message = "Xóa danh mục thành công" });
        }
    }
}
