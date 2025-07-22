

namespace PRNFE.MVC.Models.Response
{
    // Response for GET /api/Residents
    public class ResidentListResponses
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

 

    // Response for GET /api/Residents/{id} (detailed)
    public class ResidentResponses
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Sửa từ int thành string
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<VehicleResponses>? Vehicles { get; set; }
        public TemporaryStayResponses? TemporaryStay { get; set; }
        public List<SupportRequestResponses>? SupportRequests { get; set; }
        public List<RoomsResponses>? Rooms { get; set; }
    }


    public class TemporaryStayResponses
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Note { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ResidentListResponses? Resident { get; set; }
    }

    public class SupportRequestResponses
    {
        public int Id { get; set; }
        public int RequestType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ImgUrl { get; set; } // Sửa thành nullable để khớp JSON
        //public int HanndleId { get; set; }
        //public string HandleNote { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    //Response for filtered results
    public class ResidentFilterResponses
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ResidentListResponses> Data { get; set; } = new List<ResidentListResponses>();
        public List<string>? Errors { get; set; }
    }
}
