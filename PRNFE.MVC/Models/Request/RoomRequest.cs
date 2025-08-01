﻿using PRNFE.MVC.Models.Response;
using System.ComponentModel.DataAnnotations;
using static PRNFE.MVC.Models.Response.DetailsRoomResponse;

namespace PRNFE.MVC.Models.Request
{
    public class RoomRequests
    {
        [Required(ErrorMessage = "Số phòng là bắt buộc")]
        [Display(Name = "Số phòng")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tầng là bắt buộc")]
        [Display(Name = "Tầng")]
        [Range(1, 50, ErrorMessage = "Tầng phải từ 1 đến 50")]
        public int Floor { get; set; }

        [Required(ErrorMessage = "Diện tích là bắt buộc")]
        [Display(Name = "Diện tích")]
        [Range(10, 200, ErrorMessage = "Diện tích phải từ 10m² đến 200m²")]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "Loại phòng là bắt buộc")]
        [Display(Name = "Loại phòng")]
        public string RoomType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số cư dân tối đa là bắt buộc")]
        [Display(Name = "Số cư dân tối đa")]
        [Range(1, 10, ErrorMessage = "Số cư dân tối đa phải từ 1 đến 10")]
        public int MaxOccupants { get; set; }
    }
public class CreateRoomRequest
    {
        [Required(ErrorMessage = "Số phòng là bắt buộc")]
        [StringLength(10, ErrorMessage = "Số phòng không được vượt quá 10 ký tự")]
        [Display(Name = "Số phòng")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tầng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Tầng phải là số dương")]
        [Display(Name = "Tầng")]
        public int Floor { get; set; }

        [Required(ErrorMessage = "Diện tích là bắt buộc")]
        [Range(1, double.MaxValue, ErrorMessage = "Diện tích phải là số dương")]
        [Display(Name = "Diện tích (m²)")]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "RoomTypeId is required")]
        public int? RoomType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa tối đa phải là số dương")]
        [Display(Name = "Sức chứa tối đa")]
        public int MaxOpt { get; set; } = 1;

        [Display(Name = "Ghi chú")]
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string Notes { get; set; } = string.Empty;

        // Initial residents and services
        public List<int> InitialResidentIds { get; set; } = new();
        public List<int> InitialServiceIds { get; set; } = new();
        public enum RoomTypes
        {
            Single = 0,          // Phòng đơn
            Double = 1,          // Phòng đôi
            Suite = 2,           // Phòng suite
            Deluxe = 3,          // Phòng deluxe
            Family = 4,          // Phòng gia đình
            Studio = 5,          // Phòng studio
            Penthouse = 6,        // Phòng penthouse
            Other = 7           // Phòng khác
        }
    }

    public class UpdateRoomRequest
    {
        public string TenantId { get; set; } = string.Empty;

        [Required(ErrorMessage = "RoomNumber is required")]
        [StringLength(10, ErrorMessage = "RoomNumber cannot exceed 10 characters.")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Floor is required")]
        [Range(1, int.MaxValue)]
        public int Floor { get; set; }

        [Required(ErrorMessage = "Area is required")]
        [Range(1, double.MaxValue)]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "RoomType is required")]
        [Range(0, 7)]
        public int RoomType { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxOpt { get; set; }

        [Range(0, 7)]
        public int Status { get; set; }

        public List<UpdateResidentInRoomDto> Residents { get; set; } = new();
        public List<UpdateServiceInRoomDto> Services { get; set; } = new();

        public enum RoomStatus
        {
            Available = 0,          // Phòng chưa ai thuê
            Occupied = 1,           // Phòng đã có người thuê
            Reserved = 2,           // Phòng đã được đặt trước
            UnderMaintenance = 3,   // Phòng đang bảo trì
            Disabled = 4,           // Phòng không sử dụng được
            PendingCleaning = 5,    // Phòng đang chờ dọn dẹp
            ExpiringSoon = 6,       // Phòng sắp hết hạn hợp đồng
            TemporarilyLocked = 7   // Phòng tạm thời bị khóa
        }

        public enum RoomTypes
        {
            Single = 0,          // Phòng đơn
            Double = 1,          // Phòng đôi
            Suite = 2,           // Phòng suite
            Deluxe = 3,          // Phòng deluxe
            Family = 4,          // Phòng gia đình
            Studio = 5,          // Phòng studio
            Penthouse = 6,        // Phòng penthouse
            Other = 7           // Phòng khác
        }
    }

    public class UpdateResidentInRoomRequest
    {
        public int ResidentId { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày kết thúc")]
        public DateTime? LeaveDate { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string Notes { get; set; } = string.Empty;

        // For display purposes
        public ResidentResponse? Resident { get; set; }
    }

    public class UpdateServiceInRoomRequest
    {
        public int ServiceId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá tùy chỉnh phải là số không âm")]
        [Display(Name = "Giá tùy chỉnh")]
        public decimal CustomPrice { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày kết thúc")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string Notes { get; set; } = string.Empty;

        // For display purposes
        public ServiceResponses? Service { get; set; }
    }

    public class FilterRoomRequest
    {
        [StringLength(10, ErrorMessage = "Số phòng không được vượt quá 10 ký tự")]
        [Display(Name = "Số phòng")]
        public string? RoomNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Tầng phải là số dương")]
        [Display(Name = "Tầng")]
        public int? Floor { get; set; }

        [Display(Name = "Loại phòng")]
        public int? RoomType { get; set; }
        //public int? RoomTypeId { get; set; }

        [Range(0, 7, ErrorMessage = "Trạng thái không hợp lệ")]
        [Display(Name = "Trạng thái")]
        public int? Status { get; set; }

        [Display(Name = "Diện tích tối thiểu")]
        [Range(0, double.MaxValue, ErrorMessage = "Diện tích phải là số không âm")]
        public decimal? MinArea { get; set; }

        [Display(Name = "Diện tích tối đa")]
        [Range(0, double.MaxValue, ErrorMessage = "Diện tích phải là số không âm")]
        public decimal? MaxArea { get; set; }

        [Display(Name = "Sức chứa tối thiểu")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải là số dương")]
        public int? MinMaxOpt { get; set; }

        [Display(Name = "Sức chứa tối đa")]
        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải là số dương")]
        public int? MaxMaxOpt { get; set; }

        [Display(Name = "Có cư dân")]
        public bool? HasResidents { get; set; }

        [Display(Name = "Có dịch vụ")]
        public bool? HasServices { get; set; }

        [Display(Name = "Ngày tạo từ")]
        [DataType(DataType.Date)]
        public DateTime? CreatedFrom { get; set; }

        [Display(Name = "Ngày tạo đến")]
        [DataType(DataType.Date)]
        public DateTime? CreatedTo { get; set; }

        [Display(Name = "Số kết quả mỗi trang")]
        [Range(1, 100, ErrorMessage = "Số kết quả phải từ 1 đến 100")]
        public int PageSize { get; set; } = 10;

        [Display(Name = "Sắp xếp theo")]
        public string SortBy { get; set; } = "RoomNumber";

        [Display(Name = "Thứ tự sắp xếp")]
        public string SortOrder { get; set; } = "asc";

        public enum RoomTypes
        {
            Single = 0,          // Phòng đơn
            Double = 1,          // Phòng đôi
            Suite = 2,           // Phòng suite
            Deluxe = 3,          // Phòng deluxe
            Family = 4,          // Phòng gia đình
            Studio = 5,          // Phòng studio
            Penthouse = 6,        // Phòng penthouse
            Other = 7           // Phòng khác
        }

        public enum RoomStatus
        {
            Available = 0,          // Phòng chưa ai thuê
            Occupied = 1,           // Phòng đã có người thuê
            Reserved = 2,           // Phòng đã được đặt trước
            UnderMaintenance = 3,   // Phòng đang bảo trì
            Disabled = 4,           // Phòng không sử dụng được
            PendingCleaning = 5,    // Phòng đang chờ dọn dẹp
            ExpiringSoon = 6,       // Phòng sắp hết hạn hợp đồng
            TemporarilyLocked = 7   // Phòng tạm thời bị khóa
        }
    }

    public class BulkUpdateRoomRequest
    {
        public List<int> RoomIds { get; set; } = new();

        [Display(Name = "Trạng thái mới")]
        public int? NewStatus { get; set; }

        [Display(Name = "Loại phòng mới")]
        public int? NewRoomTypeId { get; set; }

        [Display(Name = "Tenant ID mới")]
        public string? NewTenantId { get; set; }

        [Display(Name = "Lý do thay đổi")]
        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }

    public class RoomTransferRequest
    {
        [Required(ErrorMessage = "ID phòng nguồn là bắt buộc")]
        public int FromRoomId { get; set; }

        [Required(ErrorMessage = "ID phòng đích là bắt buộc")]
        public int ToRoomId { get; set; }

        public List<int> ResidentIds { get; set; } = new();
        public List<int> ServiceIds { get; set; } = new();

        [Display(Name = "Ngày chuyển")]
        [DataType(DataType.Date)]
        public DateTime TransferDate { get; set; } = DateTime.Now;

        [Display(Name = "Lý do chuyển")]
        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string Notes { get; set; } = string.Empty;
    }

    public class RoomMaintenanceRequest
    {
        [Required(ErrorMessage = "ID phòng là bắt buộc")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Loại bảo trì là bắt buộc")]
        [Display(Name = "Loại bảo trì")]
        public string MaintenanceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [Display(Name = "Mô tả")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày dự kiến hoàn thành")]
        [DataType(DataType.Date)]
        public DateTime? EstimatedEndDate { get; set; }

        [Display(Name = "Chi phí dự kiến")]
        [Range(0, double.MaxValue, ErrorMessage = "Chi phí phải là số không âm")]
        public decimal? EstimatedCost { get; set; }

        [Display(Name = "Ưu tiên")]
        [Range(1, 5, ErrorMessage = "Ưu tiên phải từ 1 đến 5")]
        public int Priority { get; set; } = 3;

        [Display(Name = "Người thực hiện")]
        public string AssignedTo { get; set; } = string.Empty;
    }
}