using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Response
{
    public class BuildingResponse
    {
        public int Id { get; set; }
        public string ownerId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public int NumberOfFloors { get; set; }
        public int NumberOfApartments { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
