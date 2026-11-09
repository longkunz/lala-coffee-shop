using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace LalaCoffee.Data
{
	public class ApplicationUser : IdentityUser
	{
		[MaxLength(100)]
		public string FullName { get; set; }

		[MaxLength(255)]
		public string Address { get; set; }
	}
}
