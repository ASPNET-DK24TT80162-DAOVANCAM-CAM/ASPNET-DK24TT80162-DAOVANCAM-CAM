using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
        [StringLength(150)]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(20)]
        [RegularExpression("^(0|\\+84)([0-9]{9,10})$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
        [StringLength(250)]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string ShippingAddress { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        public IList<CartItem> Items { get; set; }

        public decimal Subtotal { get; set; }

        public decimal ShippingFee { get; set; }

        public decimal Total { get; set; }
    }
}