using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        private static bool IsActiveStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            return status == "1" 
                || string.Equals(status, "Show", StringComparison.OrdinalIgnoreCase) 
                || string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "True", StringComparison.OrdinalIgnoreCase);
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
            return View(new List<Product>());
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult GetData()
        {
            var dtRequest = DataTableRequest.Parse(Request);

            var query = Context.Products.Include(p => p.Category).AsNoTracking().AsQueryable();
            int totalRecords = query.Count();

            // 1. Tìm kiếm trên Server
            if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
            {
                var s = dtRequest.SearchValue.Trim();
                query = query.Where(p =>
                    (p.Name != null && p.Name.Contains(s)) ||
                    (p.Slug != null && p.Slug.Contains(s)) ||
                    (p.Category != null && p.Category.Name != null && p.Category.Name.Contains(s))
                );
            }

            int filteredRecords = query.Count();

            // 2. Sắp xếp trên Server
            bool isAsc = dtRequest.SortDirection == "asc";
            switch (dtRequest.SortColumnIndex)
            {
                case 1: // Id
                    query = isAsc ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
                    break;
                case 2: // Tên
                    query = isAsc ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
                    break;
                case 3: // Slug
                    query = isAsc ? query.OrderBy(p => p.Slug) : query.OrderByDescending(p => p.Slug);
                    break;
                case 4: // Giá
                    query = isAsc ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                    break;
                case 6: // Danh mục
                    query = isAsc ? query.OrderBy(p => p.Category != null ? p.Category.Name : string.Empty) 
                                  : query.OrderByDescending(p => p.Category != null ? p.Category.Name : string.Empty);
                    break;
                case 7: // Trạng thái
                    query = isAsc ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status);
                    break;
                case 8: // CreatedAt
                    query = isAsc ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt);
                    break;
                case 9: // UpdatedAt
                    query = isAsc ? query.OrderBy(p => p.UpdatedAt) : query.OrderByDescending(p => p.UpdatedAt);
                    break;
                default:
                    query = isAsc ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
                    break;
            }

            // 3. Phân trang trên Server
            var pagedProducts = query.Skip(dtRequest.Start).Take(dtRequest.Length).ToList();

            var data = pagedProducts.Select(item => new
            {
                id = item.Id,
                name = item.Name ?? string.Empty,
                slug = item.Slug ?? string.Empty,
                priceFormatted = item.Price.ToString("N0") + "đ",
                image = item.Image ?? string.Empty,
                categoryName = item.Category != null ? item.Category.Name : string.Empty,
                statusBadge = IsActiveStatus(item.Status) ? "<span class='btn btn-success btn-xs'>ON</span>" : "<span class='btn btn-danger btn-xs'>OFF</span>",
                createdAt = item.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                updatedAt = item.UpdatedAt.ToString("dd/MM/yyyy HH:mm"),
                editUrl = Url.Action("Edit", "ManageProduct", new { id = item.Id })
            });

            return Json(new
            {
                draw = dtRequest.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection formCollection, Product product)
        {
            var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var generalErrors = new List<string>();
            var formValues = formCollection.AllKeys.ToDictionary(k => k, k => formCollection[k] ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            var name = formCollection["name"]?.Trim();
            var slug = formCollection["slug"]?.Trim();
            var priceStr = formCollection["price"]?.Trim();
            var description = formCollection["description"]?.Trim();
            var image = formCollection["image"];
            var status = formCollection["status"] == "1" ? "1" : "0";
            var categoryIdStr = formCollection["category_id"];

            if (string.IsNullOrWhiteSpace(name))
            {
                fieldErrors["name"] = "Vui lòng nhập tên sản phẩm.";
            }
            if (string.IsNullOrWhiteSpace(slug))
            {
                fieldErrors["slug"] = "Vui lòng nhập slug sản phẩm.";
            }
            else if (Context.Products.Any(x => x.Slug == slug))
            {
                fieldErrors["slug"] = "Slug đã tồn tại, vui lòng chọn slug khác.";
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                fieldErrors["description"] = "Vui lòng nhập mô tả sản phẩm.";
            }
            decimal parsedPrice = 0;
            if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 1000)
            {
                fieldErrors["price"] = "Giá sản phẩm phải từ 1.000đ trở lên.";
            }
            int categoryId = 0;
            if (!int.TryParse(categoryIdStr, out categoryId) || _categoryRepository.GetById(categoryId) == null)
            {
                fieldErrors["category_id"] = "Vui lòng chọn danh mục hợp lệ.";
            }

            if (fieldErrors.Count > 0)
            {
                TempData["Errors"] = fieldErrors.Values.ToList();
                TempData["FieldErrors"] = fieldErrors;
                TempData["FormValues"] = formValues;
                TempData["OpenModal"] = "create_modal";
                return RedirectToAction("Index", "ManageProduct");
            }

            try
            {
                product.Name = name;
                product.Slug = slug;
                product.Description = description;
                product.Image = image;
                product.Price = parsedPrice;
                product.Status = status;
                product.Category = _categoryRepository.GetById(categoryId);
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;
                Add(product);
            }
            catch (Exception ex)
            {
                generalErrors.Add(ex.Message);
                TempData["Errors"] = generalErrors;
                TempData["FieldErrors"] = fieldErrors;
                TempData["FormValues"] = formValues;
                TempData["OpenModal"] = "create_modal";
                return RedirectToAction("Index", "ManageProduct");
            }

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
            var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var generalErrors = new List<string>();

            var idStr = formCollection["id"] ?? formCollection["Id"];
            var name = formCollection["name"]?.Trim();
            var slug = formCollection["slug"]?.Trim();
            var priceStr = formCollection["price"]?.Trim();
            var description = formCollection["description"]?.Trim();
            var image = formCollection["image"];
            var status = formCollection["status"] == "1" ? "1" : "0";
            var categoryIdStr = formCollection["category_id"];

            int productId = 0;
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out productId))
            {
                TempData["Errors"] = new List<string> { "ID sản phẩm không hợp lệ." };
                return RedirectToAction("Index", "ManageProduct");
            }

            var product = GetById(productId);
            if (product == null)
            {
                TempData["Errors"] = new List<string> { "Không tìm thấy sản phẩm cần chỉnh sửa." };
                return RedirectToAction("Index", "ManageProduct");
            }

            product.Name = name;
            product.Slug = slug;
            product.Description = description;
            product.Image = image;
            product.Status = status;

            int categoryId = 0;
            if (int.TryParse(categoryIdStr, out categoryId))
            {
                product.Category = _categoryRepository.GetById(categoryId);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                fieldErrors["name"] = "Vui lòng nhập tên sản phẩm.";
            }
            if (string.IsNullOrWhiteSpace(slug))
            {
                fieldErrors["slug"] = "Vui lòng nhập slug sản phẩm.";
            }
            else
            {
                var slugExists = Context.Products.Any(x => x.Slug == slug && x.Id != productId);
                if (slugExists)
                {
                    fieldErrors["slug"] = "Slug đã tồn tại, vui lòng chọn slug khác.";
                }
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                fieldErrors["description"] = "Vui lòng nhập mô tả sản phẩm.";
            }
            decimal parsedPrice = 0;
            if (!TryParsePrice(priceStr, out parsedPrice) || parsedPrice < 1000)
            {
                fieldErrors["price"] = "Giá sản phẩm phải từ 1.000đ trở lên.";
            }
            else
            {
                product.Price = parsedPrice;
            }
            if (product.Category == null)
            {
                fieldErrors["category_id"] = "Vui lòng chọn danh mục hợp lệ.";
            }

            if (fieldErrors.Count > 0)
            {
                ViewBag.PriceInput = priceStr;
                ViewBag.Errors = fieldErrors.Values.ToList();
                ViewBag.FieldErrors = fieldErrors;
                ViewBag.Categories = _categoryRepository.GetAll().ToList();
                return View(product);
            }

            try
            {
                product.UpdatedAt = DateTime.Now;
                Update(product);
            }
            catch (Exception ex)
            {
                generalErrors.Add(ex.Message);
                ViewBag.PriceInput = priceStr;
                ViewBag.Errors = generalErrors;
                ViewBag.FieldErrors = fieldErrors;
                ViewBag.Categories = _categoryRepository.GetAll().ToList();
                return View(product);
            }

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
            return TryRemove(product, "Xóa sản phẩm thành công", "Không thể xóa sản phẩm này vì đang có dữ liệu liên quan (kích cỡ, topping hoặc đơn hàng). Hãy xóa dữ liệu liên quan trước hoặc ẩn sản phẩm thay vì xóa.");
        }
    }
}
