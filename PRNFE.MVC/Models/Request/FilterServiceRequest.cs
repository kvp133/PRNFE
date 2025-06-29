using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class FilterServiceRequest
    {
        [Display(Name = "Tên dịch vụ")]
        public string? Name { get; set; }

        [Display(Name = "Dịch vụ bắt buộc")]
        public bool? IsMandatory { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool? IsActive { get; set; }

        [Display(Name = "Số kết quả mỗi trang")]
        [Range(5, 100, ErrorMessage = "Số kết quả mỗi trang phải từ 5 đến 100")]
        public int PageSize { get; set; } = 10;
    }
}