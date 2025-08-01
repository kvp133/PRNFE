﻿using static PRNFE.MVC.Models.Response.DetailsRoomResponse;
using System.ComponentModel.DataAnnotations;

namespace PRNFE.MVC.Models.Request
{
        public class UpdateRoomRequests
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

       
    
}
