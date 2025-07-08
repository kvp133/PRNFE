using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Response
{
    public class RoomResponse
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public int RoomTypeId { get; set; }
        public int MaxOpt { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties for display
        public RoomTypeResponse? RoomType { get; set; }
        public TenantResponse? Tenant { get; set; }
    }

    public class DetailRoomResponse : RoomResponse
    {
        public List<ResidentInRoomResponse>? Residents { get; set; }
        public List<ServiceInRoomResponse>? Services { get; set; }
        public List<RoomHistoryResponse>? History { get; set; }
        public RoomStatisticsResponse? Statistics { get; set; }
    }

    public class ResidentInRoomResponse
    {
        public int ResidentId { get; set; }
        public bool IsActive { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public ResidentResponse? Resident { get; set; }
    }

    public class ServiceInRoomResponse
    {
        public int ServiceId { get; set; }
        public decimal CustomPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public ServiceResponse? Service { get; set; }
    }

    public class ResidentResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string IdentityCard { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RoomTypeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int MaxOccupants { get; set; }
        public List<string> Amenities { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class TenantResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RoomHistoryResponse
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class RoomStatisticsResponse
    {
        public int TotalResidents { get; set; }
        public int ActiveResidents { get; set; }
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public decimal TotalServiceCost { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int OccupancyRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}