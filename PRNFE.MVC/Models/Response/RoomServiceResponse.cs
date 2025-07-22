namespace PRNFE.MVC.Models.Response
{
    public class RoomServiceResponses
    {
        public Guid RoomServiceId { get; set; }
        public Guid RoomId { get; set; }
        public Guid ServiceId { get; set; }
        public bool IsActive { get; set; }
        public decimal? CustomPrice { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal DefaultPrice { get; set; }
    }
}
