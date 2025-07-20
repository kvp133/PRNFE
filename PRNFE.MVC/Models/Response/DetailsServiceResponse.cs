namespace PRNFE.MVC.Models.Response
{
    public class DetailsServiceResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal PricePerUnit { get; set; }

        public bool IsMandatory { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<RoomInServiceResponse> Rooms { get; set; }
       
    }
    public class RoomInServiceResponse
    {
        public int RoomId { get; set; }
        public decimal CustomPrice { get; set; }

        public RoomResponse? Room { get; set; }
    }

}
