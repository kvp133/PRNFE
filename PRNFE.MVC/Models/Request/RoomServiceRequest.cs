using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class RoomServiceRequest
    {
        [Required(ErrorMessage = "Phòng là bắt buộc")]
        [Display(Name = "Phòng")]
        public Guid RoomId { get; set; }

        [Required(ErrorMessage = "Dịch vụ là bắt buộc")]
        [Display(Name = "Dịch vụ")]
        public Guid ServiceId { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Giá tùy chỉnh")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? CustomPrice { get; set; }
    }
}
