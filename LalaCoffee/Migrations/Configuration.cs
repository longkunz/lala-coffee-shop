namespace LalaCoffee.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using LalaCoffee.Models;
    using LalaCoffee.Data;

    internal sealed class Configuration : DbMigrationsConfiguration<LalaCoffee.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(LalaCoffee.Models.ApplicationDbContext context)
        {
            // 1. Roles
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }
            if (!roleManager.RoleExists("User"))
            {
                roleManager.Create(new IdentityRole("User"));
            }

            // 2. Admin User
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var adminUser = userManager.FindByName("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@lalacoffee.vn",
                    EmailConfirmed = true,
                    PhoneNumber = "0901234567",
                    PhoneNumberConfirmed = true,
                    FullName = "Quản trị viên",
                    Address = "738 Võ Văn Kiệt, phường An Đông, TPHCM"
                };
                var createResult = userManager.Create(adminUser, "Admin@123");
                if (createResult.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Admin");
                }
            }

            // 3. Categories
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Cà Phê Truyền Thống", Slug = "ca-phe-truyen-thong", Description = "Hương vị Robusta Đắk Lắk đậm đà nguyên bản.", ParentId = 0, Status = "Show" },
                new Category { Id = 2, Name = "Cà Phê Pha Máy", Slug = "ca-phe-pha-may", Description = "Espresso, Latte, Caramel Macchiato chuẩn vị Ý.", ParentId = 0, Status = "Show" },
                new Category { Id = 3, Name = "Cold Brew Đặc Sản", Slug = "cold-brew", Description = "Cà phê ủ lạnh 16 tiếng thanh mát tự nhiên.", ParentId = 0, Status = "Show" },
                new Category { Id = 4, Name = "Trà Trái Cây & Macchiato", Slug = "tra-trai-cay", Description = "Trà hoa quả tươi mát kết hợp lớp bọt kem cheese béo mịn.", ParentId = 0, Status = "Show" },
                new Category { Id = 5, Name = "Trà Sữa & Đá Xay", Slug = "tra-sua-da-xay", Description = "Trà sữa Oolong nướng thơm ngậy và đá xay giải nhiệt.", ParentId = 0, Status = "Show" },
                new Category { Id = 6, Name = "Bánh Mì & Tráng Miệng", Slug = "banh-mi-trang-mieng", Description = "Bánh mì Việt Nam giòn rụm và bánh ngọt cao cấp.", ParentId = 0, Status = "Show" }
            };

            foreach (var c in categories)
            {
                context.Categories.AddOrUpdate(x => x.Id, c);
            }

            context.SaveChanges();
        }
    }
}