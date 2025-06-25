using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class RoomRequest
    {
        [Required(ErrorMessage = "Số phòng là bắt buộc")]
        [Display(Name = "Số phòng")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tầng là bắt buộc")]
        [Display(Name = "Tầng")]
        [Range(1, 50, ErrorMessage = "Tầng phải từ 1 đến 50")]
        public int Floor { get; set; }

        [Required(ErrorMessage = "Diện tích là bắt buộc")]
        [Display(Name = "Diện tích")]
        [Range(10, 200, ErrorMessage = "Diện tích phải từ 10m² đến 200m²")]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "Loại phòng là bắt buộc")]
        [Display(Name = "Loại phòng")]
        public string RoomType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số cư dân tối đa là bắt buộc")]
        [Display(Name = "Số cư dân tối đa")]
        [Range(1, 10, ErrorMessage = "Số cư dân tối đa phải từ 1 đến 10")]
        public int MaxOccupants { get; set; }
    }
}