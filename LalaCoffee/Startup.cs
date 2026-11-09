using LalaCoffee.Data;
using LalaCoffee.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(LalaCoffee.Startup))]

namespace LalaCoffee
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Đăng ký per-Owin-context để ApplicationUserManager.Create thực sự
            // được gọi và DataProtectorTokenProvider có hiệu lực: trước đây chỉ
            // cấu hình cookie nên Create không bao giờ chạy (dead code) và
            // Create còn phụ thuộc context.Get<ApplicationDbContext>() — thiếu
            // đăng ký DbContext per-Owin thì gọi Create sẽ lỗi ngay.
            // Thứ tự: DbContext trước UserManager.
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });


            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
