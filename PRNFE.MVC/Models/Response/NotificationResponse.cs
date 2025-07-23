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

        public List<NotificationTargetResponse>? NotificationTargets { get; set; }
    }

    public class NotificationTargetResponse
    {
        public int ResidentId { get; set; }
        public int Status { get; set; }
        public string? ResidentEmail { get; set; }
    }
}
