namespace PRNFE.MVC.Models.Response
{
    public class RoomResponse
    {
        public Guid RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public int MaxOccupants { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
