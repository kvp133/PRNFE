namespace PRNFE.MVC.Models.Response
{
    public class ResidentResponse
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public bool Gender { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
