namespace PRNFE.MVC.Models.Response.dat
{
	public class ServiceResponseDat
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Unit { get; set; }
		public string PricePerUnit { get; set; }
		public string IsMandatory { get; set; }
		public string IsActive { get; set; }
		public DateTime CreateAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
