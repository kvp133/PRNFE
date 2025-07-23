using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class UpdateNotificationRequest
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "Loại mục tiêu là bắt buộc")]
        [Range(0, 2, ErrorMessage = "Loại mục tiêu không hợp lệ")]
        public int? TypeTarget { get; set; }

        [Required(ErrorMessage = "Ngày đăng là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime PublishDate { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Range(0, 2, ErrorMessage = "Trạng thái không hợp lệ")]
        public int? Status { get; set; }

        public List<int>? RoomIds { get; set; }
        public List<int>? ResidentIds { get; set; }
    }
}
