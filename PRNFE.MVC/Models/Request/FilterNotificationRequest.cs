using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{

    public class FilterNotificationRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        [Range(0, 2, ErrorMessage = "BuildingId must be from 0 to 2.")]
        public int? TypeTarget { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Date follow format yy-mm-dd.")]
        public DateTime? PublishDate { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Date follow format yy-mm-dd.")]
        public DateTime? FromDate { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Date follow format yy-mm-dd.")]
        public DateTime? ToDate { get; set; }
        [Range(0, 2, ErrorMessage = "Status must be from 0 to 2.")]
        public int? Status { get; set; }

        public enum NotificationTypeTarget
        {
            Building = 0,
            Room = 1,
            Resident = 2
        }
        public enum NotificationStatus
        {
            Pending = 0,
            Published = 1,
            Archived = 2
        }
    }

}
