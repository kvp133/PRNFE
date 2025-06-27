namespace PRNFE.MVC.Models.Response
{
    // không được xóa
    public class VehicleResponse
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
