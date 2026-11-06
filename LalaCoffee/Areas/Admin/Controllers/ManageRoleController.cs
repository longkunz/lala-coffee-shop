using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LalaCoffee.Controllers;
using LalaCoffee.Data;
using LalaCoffee.Models;

namespace LalaCoffee.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageRoleController : BaseController<IdentityRole>
    {
        private static readonly HashSet<string> SystemRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "User"
        };

        // GET: Admin/ManageRole
        public ActionResult Index()
        {
            var roles = GetAll().ToList();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection formCollection)
        {
            var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var generalErrors = new List<string>();
            var formValues = formCollection.AllKeys.ToDictionary(k => k, k => formCollection[k] ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            var name = formCollection["name"]?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                fieldErrors["name"] = "Vui lòng nhập tên vai trò.";
            }
            else if (SystemRoles.Contains(name))
            {
                fieldErrors["name"] = $"Vai trò '{name}' là vai trò hệ thống đã tồn tại.";
            }
            else
            {
                var checkRole = Context.Roles.Any(x => x.Name.ToLower() == name.ToLower());
                if (checkRole)
                {
                    fieldErrors["name"] = "Tên vai trò đã tồn tại, vui lòng chọn tên khác.";
                }
            }

            if (fieldErrors.Count > 0)
            {
                TempData["Errors"] = fieldErrors.Values.ToList();
                TempData["FieldErrors"] = fieldErrors;
                TempData["FormValues"] = formValues;
                TempData["OpenModal"] = "create_modal";
                return RedirectToAction("Index", "ManageRole");
            }

            try
            {
                IdentityRole role = new IdentityRole(name);
                Context.Roles.Add(role);
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                generalErrors.Add(ex.Message);
                TempData["Errors"] = generalErrors;
                TempData["FieldErrors"] = fieldErrors;
                TempData["FormValues"] = formValues;
                TempData["OpenModal"] = "create_modal";
                return RedirectToAction("Index", "ManageRole");
            }

            return RedirectToAction("Index", "ManageRole");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "ID không hợp lệ." });
            }
            var role = Context.Roles.FirstOrDefault(x => x.Id == id);
            if (role == null)
            {
                return Json(new { success = false, message = "Role không tồn tại." });
            }

            // Bảo vệ role hệ thống (Admin, User)
            if (SystemRoles.Contains(role.Name))
            {
                return Json(new { success = false, message = $"Role '{role.Name}' là role hệ thống, không thể xóa!" });
            }

            // Kiểm tra role có đang được gán cho người dùng nào không
            var hasUsers = Context.Set<IdentityUserRole>().Any(ur => ur.RoleId == role.Id);
            if (hasUsers)
            {
                return Json(new { success = false, message = $"Không thể xóa role '{role.Name}' vì đang có người dùng được gán role này." });
            }

            Remove(role);
            return Json(new { success = true, message = "Xóa role thành công." });
        }

        public ActionResult SetRole()
        {
            var roles = Context.Roles.ToList();
            var users = Context.Users.ToList();

            var userIds = new HashSet<string>(users.Select(u => u.Id));
            var roleIds = new HashSet<string>(roles.Select(r => r.Id));

            // Single query to get all user-role mappings, filtered in-memory against valid users and roles
            var userRoles = Context.Set<IdentityUserRole>().ToList();
            var roleUserList = userRoles
                .Where(ur => userIds.Contains(ur.UserId) && roleIds.Contains(ur.RoleId))
                .Select(ur => new AspNetUserRoles { RoleId = ur.RoleId, UserId = ur.UserId })
                .ToList();

            ViewBag.Roles = roles;
            ViewBag.Users = users;
            return View(roleUserList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetRole(FormCollection formCollection)
        {
            var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var generalErrors = new List<string>();
            var formValues = formCollection.AllKeys.ToDictionary(k => k, k => formCollection[k] ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            var RoleId = formCollection["RoleId"];
            var UserId = formCollection["UserId"];

            var getRole = Context.Roles.FirstOrDefault(x => x.Id == RoleId);
            var getUser = Context.Users.FirstOrDefault(x => x.Id == UserId);

            if (string.IsNullOrWhiteSpace(RoleId) || getRole == null)
            {
                fieldErrors["RoleId"] = "Vui lòng chọn vai trò hợp lệ.";
            }
            if (string.IsNullOrWhiteSpace(UserId) || getUser == null)
            {
                fieldErrors["UserId"] = "Vui lòng chọn người dùng hợp lệ.";
            }

            if (fieldErrors.Count == 0)
            {
                var alreadyAssigned = Context.Set<IdentityUserRole>().Any(ur => ur.UserId == getUser.Id && ur.RoleId == getRole.Id);
                if (alreadyAssigned)
                {
                    fieldErrors["RoleId"] = $"Người dùng '{getUser.UserName}' đã được gán vai trò '{getRole.Name}' rồi.";
                }
            }

            if (fieldErrors.Count > 0)
            {
                TempData["Errors"] = fieldErrors.Values.ToList();
                TempData["FieldErrors"] = fieldErrors;
                TempData["FormValues"] = formValues;
                TempData["OpenModal"] = "create_modal";
                return RedirectToAction("SetRole", "ManageRole");
            }

            try
            {
                IdentityUserRole userRole = new IdentityUserRole();
                userRole.UserId = getUser.Id;
                userRole.RoleId = getRole.Id;
                getUser.Roles.Add(userRole);
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                generalErrors.Add(ex.Message);
                TempData["Errors"] = generalErrors;
                TempData["FieldErrors"] = fieldErrors;
                TempData["FormValues"] = formValues;
                TempData["OpenModal"] = "create_modal";
                return RedirectToAction("SetRole", "ManageRole");
            }

            return RedirectToAction("SetRole", "ManageRole");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSetRole(string UserId, string RoleId, bool confirmSelfRevoke = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(RoleId))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
                }
                var getRole = Context.Roles.FirstOrDefault(x => x.Id == RoleId);
                var getUser = Context.Users.FirstOrDefault(x => x.Id == UserId);

                if (getRole == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy Role này." });
                }
                if (getUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy User này." });
                }

                var userRole = Context.Set<IdentityUserRole>().FirstOrDefault(ur => ur.UserId == getUser.Id && ur.RoleId == getRole.Id);
                if (userRole == null)
                {
                    return Json(new { success = false, message = "Người dùng không được gán quyền này." });
                }

                bool isRoleAdmin = string.Equals(getRole.Name, "Admin", StringComparison.OrdinalIgnoreCase);

                // 1. Chặn gỡ quyền Admin của quản trị viên cuối cùng trong hệ thống
                if (isRoleAdmin)
                {
                    var adminCount = Context.Set<IdentityUserRole>().Count(ur => ur.RoleId == getRole.Id);
                    if (adminCount <= 1)
                    {
                        return Json(new { 
                            success = false, 
                            message = "Không thể gỡ quyền Admin của quản trị viên cuối cùng trong hệ thống!" 
                        });
                    }
                }

                // 2. Cảnh báo khi tự gỡ quyền
                var currentUserId = User?.Identity?.GetUserId();
                bool isSelf = !string.IsNullOrEmpty(currentUserId) && string.Equals(getUser.Id, currentUserId, StringComparison.OrdinalIgnoreCase);

                if (isSelf && !confirmSelfRevoke)
                {
                    string warningMessage = isRoleAdmin
                        ? "CẢNH BÁO NGUY HIỂM: Bạn đang tự gỡ quyền Admin của chính tài khoản mình! Bạn sẽ mất quyền truy cập trang quản trị ngay lập tức. Bạn có chắc chắn muốn tiếp tục?"
                        : $"Bạn đang tự gỡ quyền '{getRole.Name}' của chính tài khoản mình. Bạn có chắc chắn muốn tiếp tục?";

                    return Json(new { 
                        success = false, 
                        requireSelfRevokeConfirm = true, 
                        isSelfRevoke = isSelf,
                        message = warningMessage 
                    });
                }

                // Tiến hành gỡ quyền
                Context.Set<IdentityUserRole>().Remove(userRole);
                Context.SaveChanges();

                return Json(new { 
                    success = true, 
                    isSelfRevoke = isSelf && isRoleAdmin,
                    message = isSelf && isRoleAdmin 
                        ? "Bạn đã tự gỡ quyền Admin thành công. Bạn sẽ được chuyển hướng về trang chủ." 
                        : "Xóa set role thành công." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
