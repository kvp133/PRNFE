using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class FilterServiceRequest
    {
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }
        public bool? IsMandatory { get; set; }
    }
}