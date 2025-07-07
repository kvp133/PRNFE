using System;
using System.Collections.Generic;

namespace PRNFE.MVC.Models.Response
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<RoleResponse> Roles { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 