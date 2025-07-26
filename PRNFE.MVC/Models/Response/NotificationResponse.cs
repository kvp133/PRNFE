// Models/Response/NotificationResponse.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Response
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public int? BuildingId { get; set; }
        [Display(Name = "Tiêu đề")]
        public string? Title { get; set; }
        [Display(Name = "Nội dung")]

        public string? Content { get; set; }
        [Display(Name = "Kiểu thông báo")]

        public int? TypeTarget { get; set; }
        [Display(Name = "Ngày thông báo")]
        [DataType(DataType.Date)]
        public DateTime PublishDate { get; set; }
        [Display(Name = "Trạng thái")]
        public int? Status { get; set; }
        [Display(Name = "Ngày tạo")]
        public DateTime CreateAt { get; set; }
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

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
