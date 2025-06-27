namespace PRNFE.MVC.Models.Response
{
 
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
