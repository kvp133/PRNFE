using PRNFE.MVC.Models.EnumClass;
using PRNFE.MVC.Models.Request;

namespace PRNFE.MVC.Models.Response.dat
{
	public class BillResponseDat
	{
		public string Id { get; set; }
		public string RoomId { get; set; }
		public string Amount { get; set; }
		public DateTime DueDate { get; set; }
		public string Status { get; set; }  // dạng string nhận từ API
		public DateTime CreateAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public RoomResponseDat Room { get; set; }
		public List<BillDetailResponseDat> BillDetails { get; set; }

	
	}
}
