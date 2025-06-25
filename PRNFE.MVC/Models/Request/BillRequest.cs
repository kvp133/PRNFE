using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
    public class BillRequest
    {
        [Required(ErrorMessage = "Phòng là bắt buộc")]
        [Display(Name = "Phòng")]
        public Guid RoomId { get; set; }

        [Required(ErrorMessage = "Tháng hóa đơn là bắt buộc")]
        [Display(Name = "Tháng hóa đơn")]
        [DataType(DataType.Date)]
        public DateTime BillMonth { get; set; }

        [Required(ErrorMessage = "Tổng tiền là bắt buộc")]
        [Display(Name = "Tổng tiền")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Pending";
    }
}
