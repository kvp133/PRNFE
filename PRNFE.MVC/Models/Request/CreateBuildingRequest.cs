using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class CreateBuildingRequest
    {

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Address { get; set; }
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Number of floors must be greater than 0")]
        public int NumberOfFloors { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Number of apartments must be greater than 0")]
        public int NumberOfApartments { get; set; }
        public List<CreateRoomRequest>? Rooms { get; set; }
        public List<CreateServiceRequest>? Services { get; set; }
    }

    public class CreateBuildingRequests { 

        [Required(ErrorMessage = "Tên tòa nhà là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Địa chỉ không được vượt quá 100 ký tự")]
        public string Address { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số tầng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số tầng phải lớn hơn 0")]
        public int NumberOfFloors { get; set; }

        [Required(ErrorMessage = "Số căn hộ là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số căn hộ phải lớn hơn 0")]
        public int NumberOfApartments { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
