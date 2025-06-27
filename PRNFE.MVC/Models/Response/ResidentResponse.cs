namespace PRNFE.MVC.Models.Response
{
    // Response for GET /api/Residents
    public class ResidentListResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Response for GET /api/Residents/{id} (detailed)
    public class ResidentResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<VehicleResponse>? Vehicles { get; set; }
        public TemporaryStayResponse? TemporaryStay { get; set; }
        public List<SupportRequestResponse>? SupportRequests { get; set; }
        public List<RoomsResponse>? Rooms { get; set; }
    }

    public class TemporaryStayResponse
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Note { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ResidentResponse? Resident { get; set; }
    }

    public class SupportRequestResponse
    {
        public int Id { get; set; }
        public int RequestType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImgUrl { get; set; } = string.Empty;
        public int HanndleId { get; set; }
        public string HandleNote { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    //Response for filtered results
    public class ResidentFilterResponse : List<ResidentListResponse>
    {
        // Không cần property Data nữa
    }
}
