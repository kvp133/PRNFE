using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class ServiceRequest
    {
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
        [Display(Name = "Tên dịch vụ")]
        public string ServiceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
        [Display(Name = "Đơn vị tính")]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá mỗi đơn vị là bắt buộc")]
        [Display(Name = "Giá mỗi đơn vị")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal PricePerUnit { get; set; }

        [Display(Name = "Bắt buộc sử dụng")]
        public bool IsMandatory { get; set; }

        [Display(Name = "Tính theo đầu người")]
        public bool IsPerResident { get; set; }
    }
}
