namespace PRNFE.MVC.Models.Response
{
    public class DetailServiceResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal PricePerUnit { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<RoomServiceResponse>? Rooms { get; set; }
    }

    public class RoomServiceResponse
    {
        public int Id { get; set; }
        public decimal CustomPrice { get; set; }
        public RoomInfo? Room { get; set; }
    }

    public class RoomInfo
    {
        public string RoomNumber { get; set; } = string.Empty;
        public int? Floor { get; set; }
    }
}