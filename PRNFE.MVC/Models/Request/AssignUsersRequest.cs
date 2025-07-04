namespace PRNFE.MVC.Models.Request
{
    public class AssignUsersRequest
    {
        public int RoleId { get; set; }
        public List<string> UserIds { get; set; } = new List<string>();
    }
} 