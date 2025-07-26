namespace PRNFE.MVC.Models.Request
{
    public class UserActionRequest
    {
        public string userName { get; set; } = string.Empty;
    }

    public class AdminResetPasswordRequest
    {
        public string userNameOrEmail { get; set; } = string.Empty;
    }
} 