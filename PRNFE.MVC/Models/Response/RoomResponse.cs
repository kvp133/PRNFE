namespace PRNFE.MVC.Models.Response
{

    public class RoomResponse
    {
        public Guid RoomId { get; set; }
        public string RoomNumber { get; set; }
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public string RoomType { get; set; }
        public int MaxOccupants { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // không được xóa 
    public class RoomsResponse
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public double? Area { get; set; }
        public bool IsActive { get; set; }
    }

  
}
