namespace PRNFE.MVC.Models.Response
{

	public class ServiceResponses
	{

		public int Id { get; set; }
		public Guid ServiceId { get; set; }
		public string Name { get; set; }
		public string Unit { get; set; }

		public decimal PricePerUnit { get; set; }

		public bool IsMandatory { get; set; }

		public bool IsActive { get; set; }
		public bool IsPerResident { get; set; }
	}
	public class ServiceResponse
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Unit { get; set; }
		public decimal PricePerUnit { get; set; }
		public bool IsMandatory { get; set; }
		public bool IsActive { get; set; }

		public DateTime CreateAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

	}
}