using System.ComponentModel.DataAnnotations;

namespace LalaCoffee.Models
{
	public class ContactViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập họ và tên của bạn")]
		[Display(Name = "Họ và tên")]
		public string FullName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
		[EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng")]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		[Display(Name = "Số điện thoại")]
		public string PhoneNumber { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn")]
		[Display(Name = "Lời nhắn")]
		public string Message { get; set; }
	}
}
