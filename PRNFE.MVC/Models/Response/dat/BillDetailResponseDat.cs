namespace PRNFE.MVC.Models.Response.dat
{
	public class BillDetailResponseDat
	{
		public string Id { get; set; }
		public string ServiceId { get; set; }
		public string Quantity { get; set; }
		public string UnitPrice { get; set; }
		public DateTime CreateAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public ServiceResponseDat Service { get; set; }
	}

}
