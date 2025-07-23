using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class CreateServiceRequest
    {

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Đơn vị là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Đơn vị không được vượt quá 50 ký tự")]
        public string Unit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số không âm")]
        public decimal PricePerUnit { get; set; }

        public bool IsMandatory { get; set; } = false;
        public bool IsActive { get; set; }

    }

  

}