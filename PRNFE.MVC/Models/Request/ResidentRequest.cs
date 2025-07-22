using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class ResidentRequests
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        [Display(Name = "User ID")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Giới tính")]
        public bool Gender { get; set; } // true = Nam, false = Nữ

        public List<VehicleCreateDtos> Vehicles { get; set; } = new List<VehicleCreateDtos>();

        public TemporaryStayCreateDtos? TemporaryStay { get; set; }

        public List<RoomCreateDtos> Rooms { get; set; } = new List<RoomCreateDtos>();
    }

    public class ResidentUpdateRequests
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Giới tính")]
        public bool Gender { get; set; }

        public List<RoomCreateDtos> Rooms { get; set; } = new List<RoomCreateDtos>();

        public List<VehicleUpdateDtos> Vehicles { get; set; } = new List<VehicleUpdateDtos>();

        public TemporaryStayUpdateDtos? TemporaryStay { get; set; }
    }


    public class TemporaryStayCreateDtos
    {
        public int ResidentId { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; } = string.Empty;

    }

    public class TemporaryStayUpdateDtos
    {
        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Display(Name = "Trạng thái")]
        public int Status { get; set; } // 0 = Chờ duyệt, 1 = Đã duyệt, 2 = Từ chối
    }

    public class RoomCreateDtos
    {
        [Required(ErrorMessage = "Room ID là bắt buộc")]
        [Display(Name = "Room ID")]
        public int RoomId { get; set; }
    }

    // Filter model for GET /api/Residents/filters
public class ResidentFilterRequests
    {
        [Display(Name = "Room IDs")]
        [EnsureValidRoomIds(ErrorMessage = "Room IDs phải là các số nguyên dương")]
        public int[]? RoomIds { get; set; } = null;

        [Display(Name = "Họ tên")]
        public string? FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Trang")]
        [Range(1, int.MaxValue, ErrorMessage = "Trang phải lớn hơn 0")]
        public int Page { get; set; } = 1;

        [Display(Name = "Kích thước trang")]
        [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100")]
        public int PageSize { get; set; } = 10;
    }

    public class EnsureValidRoomIdsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is int[] roomIds && roomIds != null)
            {
                if (roomIds.Any(id => id <= 0))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }

    public class ResidentDeleteRequests
    {
        [Required(ErrorMessage = "ID là bắt buộc")]
        public int Id { get; set; }

    }
}
