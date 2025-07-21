using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Response
{
    public class BuildingResponse
    {
        public int Id { get; set; }
        public int? OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfFloors { get; set; }
        public int NumberOfApartments { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
