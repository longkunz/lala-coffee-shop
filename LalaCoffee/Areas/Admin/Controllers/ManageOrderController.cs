using LalaCoffee.Controllers;
using LalaCoffee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LalaCoffee.Areas.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class ManageOrderController : BaseController<Order>
	{
		// GET: Admin/ManageOrder
		public ActionResult Index()
		{
			return View(new List<Order>());
		}

		[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		public ActionResult GetData()
		{
			var dtRequest = DataTableRequest.Parse(Request);

			var query = Context.Orders.AsNoTracking().AsQueryable();
			int totalRecords = query.Count();

			// 1. Tìm kiếm trên Server
			if (!string.IsNullOrWhiteSpace(dtRequest.SearchValue))
			{
				var s = dtRequest.SearchValue.Trim();
				query = query.Where(o =>
					(o.Code != null && o.Code.Contains(s)) ||
					(o.Name != null && o.Name.Contains(s)) ||
					(o.PhoneNumber != null && o.PhoneNumber.Contains(s)) ||
					(o.Address != null && o.Address.Contains(s)) ||
					(o.Payment != null && o.Payment.Contains(s)) ||
					(o.Status != null && o.Status.Contains(s)) ||
					(o.Note != null && o.Note.Contains(s))
				);
			}

			int filteredRecords = query.Count();

			// 2. Sắp xếp trên Server
			bool isAsc = dtRequest.SortDirection == "asc";
			switch (dtRequest.SortColumnIndex)
			{
				case 1: // Mã đơn (Code)
					query = isAsc ? query.OrderBy(o => o.Code) : query.OrderByDescending(o => o.Code);
					break;
				case 2: // Khách hàng (Name)
					query = isAsc ? query.OrderBy(o => o.Name) : query.OrderByDescending(o => o.Name);
					break;
				case 3: // Số điện thoại (PhoneNumber)
					query = isAsc ? query.OrderBy(o => o.PhoneNumber) : query.OrderByDescending(o => o.PhoneNumber);
					break;
				case 4: // Địa chỉ (Address)
					query = isAsc ? query.OrderBy(o => o.Address) : query.OrderByDescending(o => o.Address);
					break;
				case 5: // PTTT (Payment)
					query = isAsc ? query.OrderBy(o => o.Payment) : query.OrderByDescending(o => o.Payment);
					break;
				case 6: // Thanh toán (Paid)
					query = isAsc ? query.OrderBy(o => o.Paid) : query.OrderByDescending(o => o.Paid);
					break;
				case 7: // Ghi chú (Note)
					query = isAsc ? query.OrderBy(o => o.Note) : query.OrderByDescending(o => o.Note);
					break;
				case 8: // Tổng tiền (Total)
					query = isAsc ? query.OrderBy(o => o.Total) : query.OrderByDescending(o => o.Total);
					break;
				case 9: // Trạng thái đơn (Status)
					query = isAsc ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status);
					break;
				case 10: // Ngày tạo (CreatedAt)
					query = isAsc ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt);
					break;
				case 11: // Cập nhật (UpdatedAt)
					query = isAsc ? query.OrderBy(o => o.UpdatedAt) : query.OrderByDescending(o => o.UpdatedAt);
					break;
				default:
					query = isAsc ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt);
					break;
			}

			// 3. Phân trang trên Server
			var pagedOrders = query.Skip(dtRequest.Start).Take(dtRequest.Length).ToList();

			var data = pagedOrders.Select(item => new
			{
				id = item.Id,
				code = item.Code ?? string.Empty,
				name = item.Name ?? string.Empty,
				phoneNumber = item.PhoneNumber ?? string.Empty,
				address = item.Address ?? string.Empty,
				payment = item.Payment ?? "cash",
				paidBadge = OrderStatus.GetPaidBadge(item.Paid),
				note = item.Note ?? string.Empty,
				totalFormatted = item.Total.ToString("N0") + "đ",
				statusBadge = OrderStatus.GetStatusBadge(item.Status),
				createdAt = item.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
				updatedAt = item.UpdatedAt.ToString("dd/MM/yyyy HH:mm"),
				detailUrl = Url.Action("Detail", "ManageOrder", new { id = item.Id }),
				editUrl = Url.Action("Edit", "ManageOrder", new { id = item.Id })
			});

			return Json(new
			{
				draw = dtRequest.Draw,
				recordsTotal = totalRecords,
				recordsFiltered = filteredRecords,
				data = data
			}, JsonRequestBehavior.AllowGet);
		}

		//--Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(FormCollection formCollection, Order order)
		{
			List<string> errors = new List<string>();
			try
			{
				var name = formCollection["name"];
				var phonenumber = formCollection["phonenumber"];
				var address = formCollection["address"];
				var payment = formCollection["payment"];
				var note = formCollection["note"];
				var status = formCollection["status"];
				var paidStr = formCollection["paid"];

				if (string.IsNullOrWhiteSpace(name))
				{
					errors.Add("Chưa nhập tên người nhận");
				}
				if (errors.Count == 0)
				{
					order.Name = name;
					order.PhoneNumber = phonenumber;
					order.Address = address;
					order.Payment = string.IsNullOrWhiteSpace(payment) ? "Cash" : payment;
					order.Note = note;
					order.Status = OrderStatus.NormalizeStatus(status);
					if (!string.IsNullOrEmpty(paidStr) && int.TryParse(paidStr, out int paidVal))
					{
						order.Paid = paidVal;
					}
					else
					{
						order.Paid = OrderStatus.Unpaid;
					}
					order.CreatedAt = DateTime.Now;
					order.UpdatedAt = DateTime.Now;
					Add(order);
				}
			}
			catch (Exception ex)
			{
				errors.Add(ex.Message);
			}
			TempData["Errors"] = errors;

			return RedirectToAction("Index", "ManageOrder");
		}

		//----EDIT
		public ActionResult Edit(int? Id)
		{
			if (Id == null)
			{
				return RedirectToAction("Index", "ManageOrder");
			}
			Order order = GetById(Id.Value);
			if (order == null)
			{
				return RedirectToAction("Index", "ManageOrder");
			}
			return View(order);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(FormCollection formCollection)
		{
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();

			var idStr = formCollection["Id"] ?? formCollection["id"];
			var name = formCollection["name"]?.Trim();
			var phonenumber = formCollection["phonenumber"]?.Trim();
			var address = formCollection["address"]?.Trim();
			var payment = formCollection["payment"];
			var note = formCollection["note"]?.Trim();
			var status = formCollection["status"];
			var paidStr = formCollection["paid"];

			int orderId = 0;
			if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out orderId))
			{
				TempData["Errors"] = new List<string> { "ID đơn hàng không hợp lệ." };
				return RedirectToAction("Index", "ManageOrder");
			}

			var order = GetById(orderId);
			if (order == null)
			{
				TempData["Errors"] = new List<string> { "Không tìm thấy đơn hàng cần cập nhật." };
				return RedirectToAction("Index", "ManageOrder");
			}

			// Preserve entered values on the order model
			order.Name = name;
			order.PhoneNumber = phonenumber;
			order.Address = address;
			if (!string.IsNullOrWhiteSpace(payment))
			{
				order.Payment = payment;
			}
			order.Note = note;
			order.Status = OrderStatus.NormalizeStatus(status);
			if (!string.IsNullOrEmpty(paidStr) && int.TryParse(paidStr, out int paidVal))
			{
				order.Paid = paidVal;
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				fieldErrors["name"] = "Vui lòng nhập tên người nhận.";
			}
			if (string.IsNullOrWhiteSpace(phonenumber))
			{
				fieldErrors["phonenumber"] = "Vui lòng nhập số điện thoại người nhận.";
			}
			else if (!AccountController.ValidateVNPhoneNumber(phonenumber))
			{
				fieldErrors["phonenumber"] = "Số điện thoại không hợp lệ (cần 10 chữ số đúng định dạng Việt Nam).";
			}
			if (string.IsNullOrWhiteSpace(address))
			{
				fieldErrors["address"] = "Vui lòng nhập địa chỉ giao hàng.";
			}

			if (fieldErrors.Count > 0)
			{
				ViewBag.Errors = fieldErrors.Values.ToList();
				ViewBag.FieldErrors = fieldErrors;
				return View(order);
			}

			try
			{
				order.UpdatedAt = DateTime.Now;
				Update(order);
			}
			catch (Exception ex)
			{
				generalErrors.Add(ex.Message);
				ViewBag.Errors = generalErrors;
				ViewBag.FieldErrors = fieldErrors;
				return View(order);
			}

			return RedirectToAction("Index", "ManageOrder");
		}

		//---DELETE
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int? Id)
		{
			if (Id == null)
			{
				return Json(new { success = false, message = "ID không hợp lệ." });
			}
			Order order = GetById(Id.Value);
			if (order == null)
			{
				return Json(new { success = false, message = "Đơn hàng không tồn tại." });
			}
			return TryRemove(order, "Xóa đơn hàng thành công.", "Không thể xóa đơn hàng này vì đang có chi tiết đơn hàng liên quan. Nếu cần, hãy chuyển trạng thái đơn sang Đã hủy thay vì xóa.");
		}

		//--DETAIL
		public ActionResult Detail(int? Id)
		{
			if (Id == null)
			{
				return RedirectToAction("Index", "ManageOrder");
			}
			var order = GetById(Id.Value);
			if (order == null)
			{
				return RedirectToAction("Index", "ManageOrder");
			}
			ViewBag.Order = order;
			var orderDetail = Context.OrderDetails.Where(x => x.Order.Id == Id).ToList();
			return View(orderDetail);
		}
	}
}
