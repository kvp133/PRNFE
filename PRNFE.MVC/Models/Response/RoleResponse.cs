using System;
using System.Collections.Generic;

namespace PRNFE.MVC.Models.Response
{
    public class RoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PermissionResponse> Permissions { get; set; }
    }
} 