using LalaCoffee.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LalaCoffee.Data;
using System.Reflection.Emit;

namespace LalaCoffee.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PropertyProduct> PropertityProducts { get; set; }
        public DbSet<OptionProduct> OptionProducts { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public ApplicationDbContext() : base("DefaultConnection"){

        }
        public static ApplicationDbContext Create() { return new ApplicationDbContext(); }
        //public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        //    : base(options)
        //{

        //}
        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public System.Data.Entity.DbSet<LalaCoffee.Data.ApplicationRole> IdentityRoles { get; set; }

        //public System.Data.Entity.DbSet<LalaCoffee.Data.ApplicationUser> ApplicationUsers { get; set; }

        //public System.Data.Entity.DbSet<LalaCoffee.Data.ApplicationRole> IdentityRoles { get; set; }

        //public System.Data.Entity.DbSet<LalaCoffee.Data.ApplicationUser> ApplicationUsers { get; set; }
    }
}
