namespace PRNFE.MVC.Models.Response
{
    public class DetailsRoomResponse
    {
        public int Id { get; set; }
        public string TenantId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public decimal Area { get; set; }
        public int RoomType { get; set; }
        public int MaxOpt { get; set; }
        public int Status { get; set; }

        public ICollection<ResidentInRoomDto>? Residents { get; set; }
        public ICollection<ServiceInRoomDto>? Services { get; set; }
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
        public class ResidentInRoomDto
        {
            public int ResidentId { get; set; }
            public bool IsActive { get; set; }

            public ResidentResponse Resident { get; set; }
        }

        public class UpdateResidentInRoomDto
        {
            public int ResidentId { get; set; }
            public bool IsActive { get; set; }
        }
        public class ServiceInRoomDto
        {
            public int ServiceId { get; set; }
            public bool IsActive { get; set; }
            public double CustomPrice { get; set; }
            public ServiceResponses? Service { get; set; }
        }

        public class UpdateServiceInRoomDto
        {
            public int ServiceId { get; set; }
            public bool IsActive { get; set; }
            public double CustomPrice { get; set; }
        }
    }
}
