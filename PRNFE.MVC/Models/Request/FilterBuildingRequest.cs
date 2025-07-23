using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class FilterBuildingRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int? MinFloors { get; set; }
        public int? MaxFloors { get; set; }
        public int? MinApartments { get; set; }
        public int? MaxApartments { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortOrder { get; set; } = "asc";
        public int PageSize { get; set; } = 50;
    }

}
