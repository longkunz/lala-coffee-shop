using LalaCoffee.Controllers;
using LalaCoffee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LalaCoffee.Areas.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class ManageBlogController : BaseController<Blog>
	{
		// GET: Admin/ManageBlog
		//[Authorize(Roles = "Admin")]
		public ActionResult Index()
		{
			return View(new List<Blog>());
		}

		[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		public ActionResult GetData()
		{
			var dtRequest = DataTableRequest.Parse(Request);

			var query = Context.Blogs.AsNoTracking().AsQueryable();
			int totalRecords = query.Count();

			// 1. Tìm kiếm trên Server
			if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
			{
				var s = dtRequest.SearchValue.Trim();
				query = query.Where(b =>
					(b.Title != null && b.Title.Contains(s)) ||
					(b.Author != null && b.Author.Contains(s))
				);
			}

			int filteredRecords = query.Count();

			// 2. Sắp xếp trên Server
			bool isAsc = dtRequest.SortDirection == "asc";
			switch (dtRequest.SortColumnIndex)
			{
				case 1: // Id
					query = isAsc ? query.OrderBy(b => b.Id) : query.OrderByDescending(b => b.Id);
					break;
				case 2: // Title
					query = isAsc ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title);
					break;
				case 3: // CreatedDate
					query = isAsc ? query.OrderBy(b => b.CreatedDate) : query.OrderByDescending(b => b.CreatedDate);
					break;
				case 4: // Published
					query = isAsc ? query.OrderBy(b => b.Published) : query.OrderByDescending(b => b.Published);
					break;
				case 5: // Author
					query = isAsc ? query.OrderBy(b => b.Author) : query.OrderByDescending(b => b.Author);
					break;
				default:
					query = isAsc ? query.OrderBy(b => b.Id) : query.OrderByDescending(b => b.Id);
					break;
			}

			// 3. Phân trang trên Server
			var pagedBlogs = query.Skip(dtRequest.Start).Take(dtRequest.Length).ToList();

			var data = pagedBlogs.Select(item => new
			{
				id = item.Id,
				title = item.Title ?? string.Empty,
				createdDate = item.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
				publishedBadge = item.Published
					? "<span class='btn btn-success btn-xs'>ON</span>"
					: "<span class='btn btn-danger btn-xs'>OFF</span>",
				author = item.Author ?? string.Empty,
				image = item.Image ?? string.Empty,
				editUrl = Url.Action("Edit", "ManageBlog", new { id = item.Id })
			});

			return Json(new
			{
				draw = dtRequest.Draw,
				recordsTotal = totalRecords,
				recordsFiltered = filteredRecords,
				data = data
			}, JsonRequestBehavior.AllowGet);
		}
		[ValidateInput(false)]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(FormCollection formCollection, Blog blog)
		{
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();
			var formValues = formCollection.AllKeys.ToDictionary(k => k, k => formCollection[k] ?? string.Empty, StringComparer.OrdinalIgnoreCase);

			var Title = formCollection["Title"]?.Trim();
			var Content = formCollection["Content"]?.Trim();
			var Published = formCollection["Published"] != null && (formCollection["Published"] == "true" || formCollection["Published"] == "1" || formCollection["Published"].Contains("true"));
			var Image = formCollection["Image"] ?? formCollection["image"];

			// Tự động lấy tên người đăng từ tài khoản đang đăng nhập
			var Author = User?.Identity?.Name;
			if (string.IsNullOrWhiteSpace(Author))
			{
				Author = "Admin";
			}

			if (string.IsNullOrWhiteSpace(Title))
			{
				fieldErrors["Title"] = "Vui lòng nhập tiêu đề bài viết.";
			}
			if (string.IsNullOrWhiteSpace(Content))
			{
				fieldErrors["Content"] = "Vui lòng nhập nội dung bài viết.";
			}

			if (fieldErrors.Count > 0)
			{
				TempData["Errors"] = fieldErrors.Values.ToList();
				TempData["FieldErrors"] = fieldErrors;
				TempData["FormValues"] = formValues;
				TempData["OpenModal"] = "create_modal";
				return RedirectToAction("Index", "ManageBlog");
			}

			try
			{
				blog.Title = Title;
				blog.Content = Content;
				blog.Author = Author;
				blog.Image = Image;
				blog.Published = Published;
				blog.CreatedDate = DateTime.Now;
				Add(blog);
			}
			catch (Exception ex)
			{
				generalErrors.Add(ex.Message);
				TempData["Errors"] = generalErrors;
				TempData["FieldErrors"] = fieldErrors;
				TempData["FormValues"] = formValues;
				TempData["OpenModal"] = "create_modal";
				return RedirectToAction("Index", "ManageBlog");
			}

			return RedirectToAction("Index", "ManageBlog");
		}
		public ActionResult Edit(int? Id)
		{
			if (Id == null)
			{
				return RedirectToAction("Index", "ManageBlog");
			}
			var checkId = GetById(Id.Value);
			if (checkId == null)
			{
				return RedirectToAction("Index", "ManageBlog");
			}
			Blog blog = GetById(Id.Value);
			return View(blog);
		}
		[ValidateInput(false)]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(FormCollection formCollection)
		{
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();

			var idStr = formCollection["Id"] ?? formCollection["id"];
			var Title = formCollection["Title"]?.Trim();
			var Content = formCollection["Content"]?.Trim();
			var Published = formCollection["Published"] != null && (formCollection["Published"] == "true" || formCollection["Published"] == "1" || formCollection["Published"].Contains("true"));
			var Author = formCollection["Author"]?.Trim();
			var Image = formCollection["Image"] ?? formCollection["image"];

			int blogId = 0;
			if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out blogId))
			{
				TempData["Errors"] = new List<string> { "ID bài viết không hợp lệ." };
				return RedirectToAction("Index", "ManageBlog");
			}

			var blog = GetById(blogId);
			if (blog == null)
			{
				TempData["Errors"] = new List<string> { "Không tìm thấy bài viết cần chỉnh sửa." };
				return RedirectToAction("Index", "ManageBlog");
			}

			blog.Title = Title;
			blog.Content = Content;
			blog.Published = Published;
			blog.Author = Author;
			blog.Image = Image;

			if (string.IsNullOrWhiteSpace(Title))
			{
				fieldErrors["Title"] = "Vui lòng nhập tiêu đề bài viết.";
			}
			if (string.IsNullOrWhiteSpace(Author))
			{
				fieldErrors["Author"] = "Vui lòng nhập tên tác giả.";
			}
			if (string.IsNullOrWhiteSpace(Content))
			{
				fieldErrors["Content"] = "Vui lòng nhập nội dung bài viết.";
			}

			if (fieldErrors.Count > 0)
			{
				ViewBag.Errors = fieldErrors.Values.ToList();
				ViewBag.FieldErrors = fieldErrors;
				return View(blog);
			}

			try
			{
				Update(blog);
			}
			catch (Exception ex)
			{
				generalErrors.Add(ex.Message);
				ViewBag.Errors = generalErrors;
				ViewBag.FieldErrors = fieldErrors;
				return View(blog);
			}

			return RedirectToAction("Index", "ManageBlog");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int? Id)
		{
			if (Id == null)
			{
				return Json(new { success = false, message = "ID không hợp lệ" });
			}
			Blog blog = GetById(Id.Value);
			if (blog == null)
			{
				return Json(new { success = false, message = "Bài viết không tồn tại" });
			}
			Remove(blog);
			return Json(new { success = true, message = "Xóa bài viết thành công" });
		}
	}
}
