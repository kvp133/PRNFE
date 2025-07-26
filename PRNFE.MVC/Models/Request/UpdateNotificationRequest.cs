using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class UpdateNotificationRequest
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [Display(Name = "Nội dung")]
        [StringLength(1000, ErrorMessage = "Nội dung không quá 1000 ký tự")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "Loại đối tượng thông báo là bắt buộc")]
        [Display(Name = "Kiểu thông báo")]
        [Range(0, 2, ErrorMessage = "Loại đối tượng không hợp lệ")]
        public int? TypeTarget { get; set; }

        [Required(ErrorMessage = "Ngày đăng là bắt buộc")]
        [Display(Name = "Ngày thông báo")]
        [DataType(DataType.Date)]
        public DateTime PublishDate { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Range(0, 2, ErrorMessage = "Trạng thái không đúng.")]
        public int? Status { get; set; }

        // Optional when turn other target type to Building
        public List<int>? RoomIds { get; set; } // Used for Room TargetType
        public List<int>? ResidentIds { get; set; }  // Used for Resident TargetType


        public enum NotificationTypeTarget
        {
            [Display(Name = "Toàn bộ tòa nhà")]
            Building = 0,
            [Display(Name = "Theo phòng")]
            Room = 1,
            [Display(Name = "Theo cư dân")]
            Resident = 2
        }
        public enum NotificationStatus
        {
            [Display(Name = "Chờ duyệt")]
            Pending = 0,
            [Display(Name = "Đã duyệt")]
            Published = 1,
            [Display(Name = "Đã gửi")]
            Archived = 2
        }
    }
}
