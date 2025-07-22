using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Response
{
    public class DetailsNotificationResponse
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Building id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Building id must be a positive integer.")]
        public int? BuildingId { get; set; }

        [Required(ErrorMessage = "Room id is required")]
        [StringLength(200, ErrorMessage = "Title cant exceed over 200 characters")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters.")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "TypeTarget is required")]
        [Range(0, 3, ErrorMessage = "TypeTarget must be a valid NotificationTypeTarget value.")]
        public int? TypeTarget { get; set; } = 0;

        [Required(ErrorMessage = "PublishDate is required")]
        [DataType(DataType.Date, ErrorMessage = "PublishDate must follow the format yyyy-MM-dd.")]
        public DateTime PublishDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Range(0, 2, ErrorMessage = "Status must be a valid NotificationStatus value.")]
        public int? Status { get; set; }

        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<NotificationTargetResponse>? NotificationTargets { get; set; }
        public enum NotificationStatus
        {
            Pending = 0,
            Published = 1,
            Archived = 2
        }
        public enum NotificationTypeTarget
        {
            Building = 0,
            Room = 1,
            Resident = 2
        }
    }
}
