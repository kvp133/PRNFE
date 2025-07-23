using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{

    public class FilterNotificationRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? TypeTarget { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Status { get; set; }
    }

}
