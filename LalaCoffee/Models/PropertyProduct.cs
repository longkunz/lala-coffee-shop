using System;
using System.ComponentModel.DataAnnotations;

namespace LalaCoffee.Models
{
	public class PropertyProduct
	{
		[Key]
		public int Id { get; set; }
		public Product Product { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public string Status { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;
	}
}
