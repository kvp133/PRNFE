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
        public List<CreateRoomRequests>? Rooms { get; set; }
        public List<CreateServiceRequest>? Services { get; set; }
    }

}
