
using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Response
{
    public class RoomResponses {
        public Guid RoomId { get; set; }
        public string RoomNumber { get; set; }
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public string RoomType { get; set; }
        public int MaxOccupants { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RoomResponse{

    
        public int Id { get; set; }
        public string TenantId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public int RoomType { get; set; }
        public int MaxOpt { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }



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

    // không được xóa 
    public class RoomsResponses
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public double? Area { get; set; }
        public bool IsActive { get; set; }
    }

    }

