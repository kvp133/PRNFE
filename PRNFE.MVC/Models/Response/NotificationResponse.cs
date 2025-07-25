// Models/Response/NotificationResponse.cs
using System;
using System.Collections.Generic;

namespace PRNFE.MVC.Models.Response
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public int? BuildingId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? TypeTarget { get; set; }
        public DateTime PublishDate { get; set; }
        public int? Status { get; set; }

        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
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
