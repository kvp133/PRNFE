namespace PRNFE.MVC.Models.Request
{
 
    public class SupportRequests
    {
        public int Id { get; set; }
        public int RequestType { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SupportRequestDetailModel : SupportRequests
    {
        public string HandleNote { get; set; }
        public int ResidentId { get; set; }
        public ResidentModel Resident { get; set; }
    }

    public class ResidentModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public bool Gender { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
