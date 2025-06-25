using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class ResidentRequest
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "CMND/CCCD là bắt buộc")]
        [Display(Name = "CMND/CCCD")]
        [StringLength(12, MinimumLength = 9, ErrorMessage = "CMND/CCCD phải từ 9-12 số")]
        public string IdentityNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ thường trú là bắt buộc")]
        [Display(Name = "Địa chỉ thường trú")]
        public string PermanentAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại cư dân là bắt buộc")]
        [Display(Name = "Loại cư dân")]
        public string ResidentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phòng là bắt buộc")]
        [Display(Name = "Phòng")]
        public Guid RoomId { get; set; }

        public Guid? UserId { get; set; }
    }

}
