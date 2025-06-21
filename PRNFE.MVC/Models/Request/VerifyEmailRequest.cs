namespace PRNFE.MVC.Models.Request
{
    public class VerifyEmailRequest
    {
        public string email { get; set; }
        public string code { get; set; }
    }
} 