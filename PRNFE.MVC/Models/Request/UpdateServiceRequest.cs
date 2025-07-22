using System.ComponentModel.DataAnnotations;
using PRNFE.MVC.Models.Response;

namespace PRNFE.MVC.Models.Request
{
    public class UpdateServiceRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Unit is required")]
        [MaxLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
        public string Unit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price per unit must be a non-negative integer")]
        public decimal PricePerUnit { get; set; }
        [Required(ErrorMessage = "Building ID is required")]
        public bool IsMandatory { get; set; }
        [Required(ErrorMessage = "Building ID is required")]
        public bool IsActive { get; set; }
        public List<RoomInServiceRequest> RoomServices { get; set; }
    }
    public class RoomInServiceRequest
    {
        public int RoomId { get; set; }
        public decimal CustomPrice { get; set; }

    }
}