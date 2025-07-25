// Models/Request/CreateNotificationRequest.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class CreateNotificationRequest
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
        [Display(Name = "Phòng")]
        public List<int>? RoomIds { get; set; }
        [Display(Name = "Cư dân")]
        public List<int>? ResidentIds { get; set; }
    }
}
