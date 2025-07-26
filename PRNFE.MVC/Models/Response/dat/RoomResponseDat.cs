namespace PRNFE.MVC.Models.Response.dat
{
	public class RoomResponseDat
	{
		public string Id { get; set; }
		public string TenantId { get; set; }
		public string RoomNumber { get; set; }
		public string Floor { get; set; }
		public string Area { get; set; }
		public string RoomType { get; set; }
		public string MaxOpt { get; set; }
		public string Status { get; set; }
		public string BuildingId { get; set; }
		public DateTime CreateAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

}
