namespace PRNFE.MVC.Models.Response.dat
{
	public class VehicleResponsesDat
	{
		public string Id { get; set; }
		public string Type { get; set; }
		public string LicensePlate { get; set; } = string.Empty;
		public DateTime CreateAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
