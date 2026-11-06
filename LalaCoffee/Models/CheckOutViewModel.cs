using System;
using System.ComponentModel.DataAnnotations;

namespace LalaCoffee.Models
{
    public class CheckOutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ Tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^(0|\+84)(86|96|97|98|32|33|34|35|36|37|38|39|91|94|83|84|85|81|82|90|93|70|79|77|76|78|92|56|58|99|59|55|87)\d{7}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số Điện Thoại")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng.")]
        [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự.")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [RegularExpression("^(cash|cod)$", ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
        [Display(Name = "Phương thức thanh toán")]
        public string Payment { get; set; } = "cash";

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        public string UserId { get; set; }
    }
}
