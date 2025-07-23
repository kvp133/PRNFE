namespace PRNFE.MVC.Models.Request
{
    public class RegisterRequest
    {
        public string userName { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string dob { get; set; }
        public string provinceCode { get; set; }
        public string districtCode { get; set; }
        public string wardId { get; set; }
        public string password { get; set; }
        public string roleId { get; set; } = "1";
    }
} 