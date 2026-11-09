using LalaCoffee.Data;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

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

		static ApplicationDbContext()
		{
			Database.SetInitializer<ApplicationDbContext>(null);
		}

		public ApplicationDbContext() : base("DefaultConnection")
		{

		}
		public static ApplicationDbContext Create() { return new ApplicationDbContext(); }
		protected override void OnModelCreating(DbModelBuilder builder)
		{
			base.OnModelCreating(builder);
		}

		public System.Data.Entity.DbSet<LalaCoffee.Data.ApplicationRole> IdentityRoles { get; set; }

	}
}
