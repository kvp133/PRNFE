namespace PRNFE.MVC.Models.Response
{
    public class BillDetailResponse
    {
        public Guid BillDetailId { get; set; }
        public Guid BillId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
    }
}
