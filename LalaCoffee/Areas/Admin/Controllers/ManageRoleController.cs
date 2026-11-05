using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
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
            List<string> errors = new List<string>();
            try
            {
                var name = formCollection["name"];
                var checkRole = Context.Roles.Count(x => x.Name == name);
                if (string.IsNullOrEmpty(name))
                {
                    errors.Add("Vui lòng nhập tên role.");
                }
                if (checkRole > 0)
                {
                    errors.Add("Tên role đã tồn tại.");
                }
                if (errors.Count == 0)
                {
                    IdentityRole role = new IdentityRole();
                    role.Name = name;
                    Context.Roles.Add(role);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;
            return RedirectToAction("Index", "ManageRole");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid? Id)
        {
            if (Id == null)
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }
            var role = Context.Roles.FirstOrDefault(x => x.Id.Equals(Id.ToString()));
            if (role == null)
            {
                return Json(new { success = false, message = "Role không tồn tại" });
            }
            Remove((IdentityRole)role);
            return Json(new { success = true, message = "Xóa role thành công" });
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
            List<string> errors = new List<string>();
            try
            {
                var RoleId = formCollection["RoleId"];
                var UserId = formCollection["UserId"];
                var getRole = Context.Roles.FirstOrDefault(x => x.Id == RoleId);
                var getUser = Context.Users.FirstOrDefault(x => x.Id == UserId);
                if (getRole == null)
                {
                    errors.Add("Không tìm thấy Role này.");
                }
                if (getUser == null)
                {
                    errors.Add("Không tìm thấy User này.");
                }
                if (errors.Count == 0)
                {
                    IdentityUserRole userRole = new IdentityUserRole();
                    userRole.UserId = getUser.Id;
                    userRole.RoleId = getRole.Id;
                    getUser.Roles.Add(userRole);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
            TempData["Errors"] = errors;
            return RedirectToAction("SetRole", "ManageRole");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSetRole(Guid? UserId, Guid? RoleId)
        {
            try
            {
                if (RoleId == null || UserId == null)
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });
                }
                var getRole = Context.Roles.FirstOrDefault(x => x.Id.Equals(RoleId.ToString()));
                var getUser = Context.Users.FirstOrDefault(x => x.Id.Equals(UserId.ToString()));

                if (getRole == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy Role này." });
                }
                if (getUser == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy User này." });
                }
                var userRole = Context.Set<IdentityUserRole>().FirstOrDefault(ur => ur.UserId == getUser.Id && ur.RoleId == getRole.Id);

                if (userRole != null)
                {
                    // Remove the record from the AspNetUserRoles table
                    Context.Set<IdentityUserRole>().Remove(userRole);

                    // Save the changes to the database
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            return Json(new { success = true, message = "Xóa set role thành công" });
        }
    }
}
