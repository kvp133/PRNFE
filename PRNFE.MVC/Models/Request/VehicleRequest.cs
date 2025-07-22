using PRNFE.MVC.Models.Response;
using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{

    // Model cho API /api/Vehicles/filters
    public class VehicleFilterRequest
    {
        [Display(Name = "Mã cư dân")]
        public List<int>? ResidentIds { get; set; }

        [Display(Name = "Mã phòng")]
        public List<int>? RoomIds { get; set; }

        [Display(Name = "Loại phương tiện")]
        public int? Type { get; set; }

        [Display(Name = "Biển số")]
        public string? LicensePlate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // API POST /api/Vehicles
    public class VehicleCreateDto
    {
        public int ResidentId { get; set; }

        [Required(ErrorMessage = "Loại xe là bắt buộc")]
        [Display(Name = "Loại xe")]
        public int Type { get; set; } // 0 = Xe máy, 1 = Ô tô, 2 = Xe đạp

        [Required(ErrorMessage = "Biển số xe là bắt buộc")]
        [Display(Name = "Biển số xe")]
        public string LicensePlate { get; set; } = string.Empty;
    }

    //PUT VEHICLE
    public class VehicleUpdateDto
    {
        [Required(ErrorMessage = "Loại xe là bắt buộc")]
        [Display(Name = "Loại xe")]
        public int Type { get; set; }

        [Required(ErrorMessage = "Biển số xe là bắt buộc")]
        [Display(Name = "Biển số xe")]
        public string LicensePlate { get; set; } = string.Empty;
    }
}
