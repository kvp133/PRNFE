using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class CreateTemporaryStayDto
    {
        public int ResidentId { get; set; }
        [Required(ErrorMessage = "FromDate is required.")]
        [DataType(DataType.Date, ErrorMessage = "FromDate must follow the format yyyy-MM-dd.")]
        [Display(Name = "From Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "ToDate is required.")]
        [DataType(DataType.Date, ErrorMessage = "ToDate must follow the format yyyy-MM-dd.")]
        [Display(Name = "To Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]

        public DateTime ToDate { get; set; }
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string? Note { get; set; }
    }

    public class UpdateTemporaryStayDto
    {
        [Required(ErrorMessage = "FromDate is required.")]
        [DataType(DataType.Date, ErrorMessage = "FromDate must follow the format yyyy-MM-dd.")]
        [Display(Name = "From Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "FromDate is required.")]
        [DataType(DataType.Date, ErrorMessage = "FromDate must follow the format yyyy-MM-dd.")]
        [Display(Name = "From Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ToDate { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Range(0, 4, ErrorMessage = "Status must be between 0 and 4.")]
        public int Status { get; set; }

        public enum TemporaryStayStatus
        {
            PendingApproval = 0, // Waiting for approval  
            Active = 1,          // Approved and active  
            Completed = 2,       // Completed stay  
            Cancelled = 3,       // Cancelled by resident or admin  
            Suspended = 4        // Suspended due to policy violation  
        }
    }

    public class FilterTemporaryStayDto
    {
        [DisplayName("Residents")]
        public List<int>? ResidentIds { get; set; }
        [DisplayName("Rooms")]
        public List<int>? RoomIds { get; set; }
        [Range(0, 4, ErrorMessage = "Status must be between 0 and 4.")]
        public int? Status { get; set; }
        public enum TemporaryStayStatus
        {
            PendingApproval = 0, // Waiting for approval
            Active = 1,          // Approved and active
            Completed = 2,       // Completed stay
            Cancelled = 3,       // Cancelled by resident or admin
            Suspended = 4        // Suspended due to policy violation
        }
    }
}
