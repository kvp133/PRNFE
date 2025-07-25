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

        public ICollection<NotificationTargetDto>? NotificationTargets { get; set; }
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

    public class NotificationTargetDto
    {
        public Guid Id { get; set; }
        public int? Status { get; set; } = 0;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ResidentDto? Resident { get; set; }
        public enum NotificationTargetStatus
        {
            UnSend = 0, // Chưa gửi
            Sent = 1, // Đã gửi
        }
    }

    public class ResidentDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public bool Gender { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
