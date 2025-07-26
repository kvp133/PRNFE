using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PRNFE.MVC.Models.EnumClass;

namespace PRNFE.MVC.Models.Response.dat
{
	public class InvoiceResponseDat
	{
		public string Id { get; set; }
		public string BuildingID { get; set; }
		public string RoomID { get; set; }
		public string TotalAmount { get; set; }

		// ✅ Trường này là Enum
		[JsonConverter(typeof(StringEnumConverter))]

		public InvoiceStatus Status { get; set; }

		// ✅ Các trường DateTime giữ nguyên
		public DateTime DueDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string UpdatedBy { get; set; }

		public string BillId { get; set; }

		public PaymentResponseDat Payment { get; set; }
	}

}
