namespace PRNFE.MVC.Models.Response
{
    public class ResidentResponse
    {
        public Guid ResidentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        public string ResidentType { get; set; } = string.Empty;
        public Guid RoomId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
