using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class VehicleRequest
    {
    }

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
