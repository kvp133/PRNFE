namespace PRNFE.MVC.Models.Request
{
    public class UpdateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> PermissionIds { get; set; } = new List<string>();
    }
} 