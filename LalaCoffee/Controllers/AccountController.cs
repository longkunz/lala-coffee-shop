using LalaCoffee.Data;
using LalaCoffee.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace LalaCoffee.Controllers
{
	public class AccountController : Controller
	{
		private ApplicationDbContext _context;
		public AccountController()
		{
			_context = new ApplicationDbContext();
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Register()
		{
			return View();
		}

		public static bool ValidateVNPhoneNumber(string phoneNumber)
		{
			phoneNumber = phoneNumber.Replace("+84", "0");
			Regex regex = new Regex(@"^(0)(86|96|97|98|32|33|34|35|36|37|38|39|91|94|83|84|85|81|82|90|93|70|79|77|76|78|92|56|58|99|59|55|87)\d{7}$");
			return regex.IsMatch(phoneNumber);
		}

		public bool ValidateEmail(string email)
		{
			Regex regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
			return regex.IsMatch(email);
		}

		public bool ValidatePassword(string password)
		{
			Regex regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
			return regex.IsMatch(password);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Register(FormCollection formCollection)
		{
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();

			var Email = formCollection["Email"]?.Trim();
			var Password = formCollection["PasswordHash"];
			var PhoneNumber = formCollection["PhoneNumber"]?.Trim();
			var UserName = formCollection["UserName"]?.Trim();

			ViewBag.UserName = UserName;
			ViewBag.Email = Email;
			ViewBag.PhoneNumber = PhoneNumber;

			if (string.IsNullOrWhiteSpace(UserName))
			{
				fieldErrors["UserName"] = "Vui lòng nhập tên đăng nhập.";
			}
			else if (_context.Users.Any(x => x.UserName == UserName))
			{
				fieldErrors["UserName"] = "Tên đăng nhập đã tồn tại, vui lòng chọn tên khác.";
			}

			if (string.IsNullOrEmpty(Password))
			{
				fieldErrors["PasswordHash"] = "Vui lòng nhập mật khẩu.";
			}
			else if (!ValidatePassword(Password))
			{
				fieldErrors["PasswordHash"] = "Mật khẩu phải có ít nhất 8 ký tự (bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt).";
			}

			if (string.IsNullOrWhiteSpace(Email))
			{
				fieldErrors["Email"] = "Vui lòng nhập địa chỉ email.";
			}
			else if (!ValidateEmail(Email))
			{
				fieldErrors["Email"] = "Email không đúng định dạng.";
			}
			else if (_context.Users.Any(x => x.Email == Email))
			{
				fieldErrors["Email"] = "Email đã tồn tại, vui lòng sử dụng email khác.";
			}

			if (string.IsNullOrWhiteSpace(PhoneNumber))
			{
				fieldErrors["PhoneNumber"] = "Vui lòng nhập số điện thoại.";
			}
			else if (!ValidateVNPhoneNumber(PhoneNumber))
			{
				fieldErrors["PhoneNumber"] = "Số điện thoại không hợp lệ (cần 10 chữ số đúng định dạng Việt Nam).";
			}
			else if (_context.Users.Any(x => x.PhoneNumber == PhoneNumber))
			{
				fieldErrors["PhoneNumber"] = "Số điện thoại đã tồn tại, vui lòng sử dụng số khác.";
			}

			if (fieldErrors.Count == 0)
			{
				try
				{
					var userStore = new UserStore<ApplicationUser>(_context);

					var userManager = new UserManager<ApplicationUser>(userStore);
					var user = new ApplicationUser { UserName = UserName, Email = Email, PhoneNumber = PhoneNumber };

					var result = userManager.Create(user, Password);
					if (result.Succeeded)
					{
						try
						{
							var roleStore = new RoleStore<IdentityRole>(_context);
							var roleManager = new RoleManager<IdentityRole>(roleStore);
							if (roleManager.RoleExists("User"))
							{
								userManager.AddToRole(user.Id, "User");
							}
						}
						catch
						{
						}

						var authenticationManager = HttpContext.GetOwinContext().Authentication;
						var userIdentity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
						authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, userIdentity);

						return RedirectToAction("Index", "Home");
					}
					else
					{
						var identityError = result.Errors.FirstOrDefault();
						if (!string.IsNullOrEmpty(identityError))
						{
							generalErrors.Add(identityError);
						}
					}
				}
				catch (Exception ex)
				{
					generalErrors.Add(ex.Message);
				}
			}

			ViewBag.FieldErrors = fieldErrors;
			ViewBag.GeneralErrors = generalErrors;
			ViewBag.Errors = generalErrors;
			TempData["Errors"] = generalErrors;
			return View();
		}
		public ActionResult Login()
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Login(FormCollection formCollection)
		{
			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var generalErrors = new List<string>();

			var UserName = formCollection["UserName"]?.Trim();
			var Password = formCollection["Password"] ?? formCollection["PassWord"];
			var rememberMe = formCollection["RememberMe"] == "true" || formCollection["RememberMe"] == "on";

			ViewBag.UserName = UserName;
			ViewBag.RememberMe = rememberMe;

			if (string.IsNullOrWhiteSpace(UserName))
			{
				fieldErrors["UserName"] = "Vui lòng nhập tên đăng nhập.";
			}

			if (string.IsNullOrWhiteSpace(Password))
			{
				fieldErrors["Password"] = "Vui lòng nhập mật khẩu.";
			}

			if (fieldErrors.Count == 0)
			{
				try
				{
					var userStore = new UserStore<ApplicationUser>(_context);
					var userManager = new UserManager<ApplicationUser>(userStore);
					var user = userManager.Find(UserName, Password);
					if (user != null)
					{
						var authenticationManager = HttpContext.GetOwinContext().Authentication;

						var userIdentity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);

						authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = rememberMe }, userIdentity);
						return RedirectToAction("Index", "Home");
					}
					else
					{
						generalErrors.Add("Tên đăng nhập hoặc mật khẩu không chính xác.");
						ViewBag.LoginFailed = true;
					}
				}
				catch (Exception ex)
				{
					generalErrors.Add(ex.Message);
				}
			}

			ViewBag.FieldErrors = fieldErrors;
			ViewBag.GeneralErrors = generalErrors;
			ViewBag.Errors = generalErrors;
			TempData["Errors"] = generalErrors;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			var authenticationManager = HttpContext.GetOwinContext().Authentication;
			authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			return RedirectToAction("Index", "Home");
		}

		public ActionResult Logout()
		{
			var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
			AuthenticationManager.SignOut();
			return RedirectToAction("Index", "Home");
		}

		[Authorize]
		[ActionName("Profile")]
		public ActionResult UserProfile()
		{
			var userId = User.Identity.GetUserId();
			var user = _context.Users.Find(userId);
			if (user == null)
			{
				return RedirectToAction("Login", "Account");
			}

			var model = new UserProfileViewModel
			{
				UserName = user.UserName,
				FullName = user.FullName,
				PhoneNumber = user.PhoneNumber,
				Email = user.Email,
				Address = user.Address
			};

			return View("Profile", model);
		}

		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		[ActionName("Profile")]
		public ActionResult UserProfile(UserProfileViewModel model)
		{
			var userId = User.Identity.GetUserId();
			var user = _context.Users.Find(userId);
			if (user == null)
			{
				return RedirectToAction("Login", "Account");
			}

			model.UserName = user.UserName;

			var fieldErrors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (string.IsNullOrWhiteSpace(model.FullName))
			{
				fieldErrors["FullName"] = "Vui lòng nhập họ và tên.";
			}

			if (string.IsNullOrWhiteSpace(model.PhoneNumber))
			{
				fieldErrors["PhoneNumber"] = "Vui lòng nhập số điện thoại.";
			}
			else if (!ValidateVNPhoneNumber(model.PhoneNumber))
			{
				fieldErrors["PhoneNumber"] = "Số điện thoại không hợp lệ (cần 10 chữ số đúng định dạng Việt Nam).";
			}
			else if (_context.Users.Any(x => x.PhoneNumber == model.PhoneNumber && x.Id != userId))
			{
				fieldErrors["PhoneNumber"] = "Số điện thoại đã được tài khoản khác sử dụng.";
			}

			if (string.IsNullOrWhiteSpace(model.Email))
			{
				fieldErrors["Email"] = "Vui lòng nhập địa chỉ email.";
			}
			else if (!ValidateEmail(model.Email))
			{
				fieldErrors["Email"] = "Email không đúng định dạng.";
			}
			else if (_context.Users.Any(x => x.Email == model.Email && x.Id != userId))
			{
				fieldErrors["Email"] = "Email đã được tài khoản khác sử dụng.";
			}

			if (string.IsNullOrWhiteSpace(model.Address))
			{
				fieldErrors["Address"] = "Vui lòng nhập địa chỉ nhận hàng.";
			}

			if (fieldErrors.Count > 0)
			{
				ViewBag.ProfileFieldErrors = fieldErrors;
				return View("Profile", model);
			}

			user.FullName = model.FullName.Trim();
			user.PhoneNumber = model.PhoneNumber.Trim();
			user.Email = model.Email.Trim();
			user.Address = model.Address.Trim();

			_context.SaveChanges();

			TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
			return RedirectToAction("Profile");
		}

		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public ActionResult ChangePassword(ChangePasswordViewModel model)
		{
			var userId = User.Identity.GetUserId();
			var user = _context.Users.Find(userId);
			if (user == null)
			{
				return RedirectToAction("Login", "Account");
			}

			var profileModel = new UserProfileViewModel
			{
				UserName = user.UserName,
				FullName = user.FullName,
				PhoneNumber = user.PhoneNumber,
				Email = user.Email,
				Address = user.Address
			};

			var passwordErrors = new List<string>();

			if (string.IsNullOrEmpty(model.CurrentPassword))
			{
				passwordErrors.Add("Vui lòng nhập mật khẩu hiện tại.");
			}

			if (string.IsNullOrEmpty(model.NewPassword))
			{
				passwordErrors.Add("Vui lòng nhập mật khẩu mới.");
			}
			else if (!ValidatePassword(model.NewPassword))
			{
				passwordErrors.Add("Mật khẩu mới phải có ít nhất 8 ký tự (bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt).");
			}

			if (model.NewPassword != model.ConfirmPassword)
			{
				passwordErrors.Add("Mật khẩu xác nhận không khớp.");
			}

			if (passwordErrors.Count > 0)
			{
				ViewBag.PasswordErrors = passwordErrors;
				return View("Profile", profileModel);
			}

			var userStore = new UserStore<ApplicationUser>(_context);
			var userManager = new UserManager<ApplicationUser>(userStore);

			var result = userManager.ChangePassword(userId, model.CurrentPassword, model.NewPassword);
			if (result.Succeeded)
			{
				var refreshedUser = userManager.FindById(userId);
				var authenticationManager = HttpContext.GetOwinContext().Authentication;
				authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
				var identity = userManager.CreateIdentity(refreshedUser, DefaultAuthenticationTypes.ApplicationCookie);
				authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

				TempData["SuccessPasswordMessage"] = "Đổi mật khẩu thành công!";
				return RedirectToAction("Profile");
			}

			foreach (var err in result.Errors)
			{
				string msg = err;
				if (err.Contains("Incorrect password"))
				{
					msg = "Mật khẩu hiện tại không chính xác.";
				}
				passwordErrors.Add(msg);
			}

			ViewBag.PasswordErrors = passwordErrors;
			return View("Profile", profileModel);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_context?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
