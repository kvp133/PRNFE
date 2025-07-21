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
        public List<RoomServiceDetailResponse>? Rooms { get; set; }
    }

    public class RoomServiceDetailResponse
    {
        public int RoomId { get; set; }
        public decimal CustomPrice { get; set; }
        public RoomDetailInfo? Room { get; set; }
    }

    public class RoomDetailInfo
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public int RoomTypeId { get; set; }
        public int MaxOpt { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}