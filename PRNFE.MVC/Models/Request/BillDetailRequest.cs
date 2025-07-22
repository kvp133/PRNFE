using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class BillDetailRequest
    {
        [Required(ErrorMessage = "Hóa đơn là bắt buộc")]
        [Display(Name = "Hóa đơn")]
        public Guid BillId { get; set; }

        [Required(ErrorMessage = "Dịch vụ là bắt buộc")]
        [Display(Name = "Dịch vụ")]
        public Guid ServiceId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Display(Name = "Số lượng")]
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Đơn giá là bắt buộc")]
        [Display(Name = "Đơn giá")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal UnitPrice { get; set; }
    }
}

