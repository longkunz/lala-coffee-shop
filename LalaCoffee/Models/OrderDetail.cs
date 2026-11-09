using System.ComponentModel.DataAnnotations;

namespace LalaCoffee.Models
{
	public class OrderDetail
	{
		[Key]
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public decimal Price { get; set; }
		public decimal Total { get; set; }
		public int Quantity { get; set; }
		public string PropertyProduct { get; set; }
		public string OptionProduct { get; set; }
		public Order Order { get; set; }

	}
}