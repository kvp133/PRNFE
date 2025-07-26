using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{

    public class FilterNotificationRequest
    {
        [Display(Name = "Tiêu đề")]
        public string? Title { get; set; }
        [Display(Name = "Nội dung")]
        public string? Content { get; set; }
        [Range(0, 2, ErrorMessage = "BuildingId must be from 0 to 2.")]
        [Display(Name = "Kiểu thông báo")]
        public int? TypeTarget { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Date follow format yy-mm-dd.")]
        [Display(Name = "Ngày duyệt")]
        public DateTime? PublishDate { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Date follow format yy-mm-dd.")]
        [Display(Name = "Bắt đầu ngày")]
        public DateTime? FromDate { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Date follow format yy-mm-dd.")]
        [Display(Name = "Kết thúc ngày")]
        public DateTime? ToDate { get; set; }
        [Range(0, 2, ErrorMessage = "Status must be from 0 to 2.")]
        [Display(Name = "Trạng thái")]
        public int? Status { get; set; }

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
