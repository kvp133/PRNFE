namespace PRNFE.MVC.Models.Response
{
    public class BillResponse
    {
        public Guid BillId { get; set; }
        public Guid RoomId { get; set; }
        public DateTime BillMonth { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
    }
}
