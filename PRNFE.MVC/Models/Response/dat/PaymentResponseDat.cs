using Newtonsoft.Json.Converters;
using PRNFE.MVC.Models.EnumClass;
using System.Text.Json.Serialization;

namespace PRNFE.MVC.Models.Response.dat
{
	public class PaymentResponseDat
	{
		public string Id { get; set; }
		public string InvoiceID { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public PaymentMethod PaymentMethod { get; set; }
		public string Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string UpdatedBy { get; set; }
		public string Invoice { get; set; }
	}

}
