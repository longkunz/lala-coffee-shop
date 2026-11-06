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

            var token = userManager.GeneratePasswordResetToken(model.UserId);
            var result = userManager.ResetPassword(model.UserId, token, model.NewPassword);
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
