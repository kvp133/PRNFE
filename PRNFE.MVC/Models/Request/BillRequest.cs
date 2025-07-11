using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
   
    // ==================== RESPONSE MODELS ====================
    //  GET /api/Bills/{id}
    public class BillResponse
    {
        public string Id { get; set; } = string.Empty;
        public int? RoomId { get; set; } // Nullable to handle absence in GET /api/Bills/filter, POST /api/Bills/Rooms, POST /api/Bills/Building
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Status { get; set; }
        public BillRoomResponse Room { get; set; } = new();
        public List<BillDetailsResponse>? BillDetails { get; set; }// Chỉ có trong GET /api/Bills/{id}
    }

    /// Room Response Model - Embedded trong BillResponse
    public class BillRoomResponse
    {
        public int Id { get; set; }
        public string? TenantId { get; set; } // Có thể null
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public int RoomType { get; set; }
        public int MaxOpt { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// BillDetail Response Model - Embedded trong BillResponse
    /// Chỉ có trong GET /api/Bills/{id}
    public class BillDetailsResponse
    {
        public string Id { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public decimal Quantity { get; set; } // Đổi từ int sang decimal
        public decimal UnitPrice { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public BillServiceResponse Service { get; set; } = new();
    }

    /// Service Response Model - Embedded trong BillDetailResponse
    public class BillServiceResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal PricePerUnit { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // ==================== REQUEST MODELS ====================
    /// Bill Create Request Model
    /// Dùng cho: POST /api/Bills
    public class BillCreateRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn hoặc bằng 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày đến hạn")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Phải có ít nhất một dịch vụ")]
        public List<BillDetailCreateRequest> BillDetails { get; set; } = new();
    }
  
    /// BillDetail Create Request Model - Embedded trong BillCreateRequest
    public class BillDetailCreateRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn dịch vụ")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        public decimal UnitPrice { get; set; }
    }

    /// Bill Update Request Model
    /// Dùng cho: PUT /api/Bills/{id}
    public class BillUpdateRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn hoặc bằng 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày đến hạn")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public int Status { get; set; }

        [Required(ErrorMessage = "Phải có ít nhất một dịch vụ")]
        public List<BillDetailCreateRequest> BillDetails { get; set; } = new();
    }

    /// Rooms Create Bills Request Model
    /// Dùng cho: POST /api/Bills/Rooms
    public class RoomsBillsCreateRequest
    {
        [Required(ErrorMessage = "Phải có ít nhất một phòng")]
        public List<int> RoomIds { get; set; } = new();
    }

    // ==================== FILTER MODELS ====================
    /// Bill Filter Model
    /// Dùng cho: GET /api/Bills/filter

    public class BillFilterRequest
    {
        public List<int> RoomIds { get; set; } = new();

        public int? Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        // Client-side filtering properties
        [Display(Name = "Tìm kiếm")]
        public string SearchTerm { get; set; } = string.Empty;

        [Display(Name = "Trang")]
        public int Page { get; set; } = 1;

        [Display(Name = "Số lượng mỗi trang")]
        public int PageSize { get; set; } = 10;
    }

    // ==================== WRAPPER MODELS ====================
    /// Bill Status Enum - Chỉ dùng cho display, API dùng số
    public enum BillStatus
    {
        [Display(Name = "Chưa thanh toán")]
        Unpaid = 0,

        [Display(Name = "Đã thanh toán")]
        Paid = 1,

        [Display(Name = "Quá hạn")]
        Overdue = 2,

        [Display(Name = "Đã hủy")]
        Cancelled = 3,

        [Display(Name = "Đang xử lý")]
        Processing = 4
    }
    /// Room Types Constants
    public static class RoomTypes
    {
        public static readonly Dictionary<int, string> Types = new()
    {
        { 1, "Phòng đơn" },
        { 2, "Phòng đôi" },
        { 3, "Phòng ba" },
        { 4, "Phòng bốn" },
        { 5, "Phòng năm" }
    };
    }
}