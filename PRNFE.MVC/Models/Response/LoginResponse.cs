namespace PRNFE.MVC.Models.Response
{
    public class LoginResponse
    {
        public bool success { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string error { get; set; }
    }
} 