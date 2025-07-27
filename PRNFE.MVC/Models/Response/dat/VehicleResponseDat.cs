namespace PRNFE.MVC.Models.Response.dat
{
	public class VehicleResponseDat
	{
		public int Id { get; set; }
		public int Type { get; set; }              // Loại xe: 1 = xe máy, 2 = ô tô, v.v.
		public string LicensePlate { get; set; }   // Biển số xe
		public DateTime CreateAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
