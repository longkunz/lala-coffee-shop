using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using LalaCoffee.Controllers;
using LalaCoffee.Data;
using LalaCoffee.Models;

namespace LalaCoffee.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageUserController : BaseController<ApplicationUser>
    {
        // GET: Admin/ManageUser
        public ActionResult Index()
        {
            var users = GetAll().ToList();
            return View(users);
        }

        // GET: Admin/ManageUser/ChangePassword
        public ActionResult ChangePassword()
        {
            ViewBag.UserName = User.Identity.GetUserName();
            return View();
        }

        // POST: Admin/ManageUser/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            ViewBag.UserName = User.Identity.GetUserName();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            var userStore = new UserStore<ApplicationUser>(Context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var result = userManager.ChangePassword(userId, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = userManager.FindById(userId);
                var authenticationManager = HttpContext.GetOwinContext().Authentication;
                authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                var identity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

                ViewBag.SuccessMessage = "Đổi mật khẩu tài khoản Admin thành công!";
                ModelState.Clear();
                return View();
            }

            foreach (var error in result.Errors)
            {
                string msg = error;
                if (error.Contains("Incorrect password"))
                {
                    msg = "Mật khẩu hiện tại không chính xác.";
                }
                ModelState.AddModelError("", msg);
            }

            return View(model);
        }

        // GET: Admin/ManageUser/ResetPassword/{id}
        public ActionResult ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var user = Context.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new AdminResetPasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };
            return View(model);
        }

        // POST: Admin/ManageUser/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(AdminResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userStore = new UserStore<ApplicationUser>(Context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            // Validate mật khẩu mới TRƯỚC khi gỡ mật khẩu cũ: nếu mật khẩu mới không hợp lệ
            // mà đã RemovePassword thì tài khoản sẽ bị mất mật khẩu.
            var validationResult = userManager.PasswordValidator.ValidateAsync(model.NewPassword).GetAwaiter().GetResult();
            if (!validationResult.Succeeded)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(model);
            }

            // Admin đặt lại mật khẩu trực tiếp cho người dùng: gỡ mật khẩu cũ rồi đặt mật khẩu mới.
            // Không dùng luồng token (GeneratePasswordResetToken/ResetPassword) vì UserManager
            // tạo thủ công tại đây không có IUserTokenProvider — gọi token sẽ luôn ném
            // InvalidOperationException (lỗi 500). Luồng token chỉ dành cho người dùng tự khôi phục.
            IdentityResult result;
            if (userManager.HasPassword(model.UserId))
            {
                result = userManager.RemovePassword(model.UserId);
                if (result.Succeeded)
                {
                    result = userManager.AddPassword(model.UserId, model.NewPassword);
                }
            }
            else
            {
                result = userManager.AddPassword(model.UserId, model.NewPassword);
            }
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Đổi mật khẩu cho người dùng '{model.UserName}' thành công!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(model);
        }
    }
}
