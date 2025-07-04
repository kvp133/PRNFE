namespace PRNFE.MVC.Models.Request
{
    public class AssignPermissionsRequest
    {
        public int RoleId { get; set; }
        public List<string> PermissionIds { get; set; } = new List<string>();
    }
} 