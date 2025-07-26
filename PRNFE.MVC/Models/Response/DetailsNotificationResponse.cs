using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Response
{
    public class DetailsNotificationResponse
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Building id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Building id must be a positive integer.")]
        public int? BuildingId { get; set; }

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

        [Display(Name = "Ngày tạo")]
        public DateTime CreateAt { get; set; }
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        public ICollection<NotificationTargetDto>? NotificationTargets { get; set; }

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

    public class NotificationTargetDto
    {
        public Guid Id { get; set; }
        public int? Status { get; set; } = 0;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ResidentDto? Resident { get; set; }
        public enum NotificationTargetStatus
        {
            UnSend = 0, // Chưa gửi
            Sent = 1, // Đã gửi
        }
    }

    public class ResidentDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public bool Gender { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
