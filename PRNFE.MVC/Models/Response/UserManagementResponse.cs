using System;
using System.Collections.Generic;

namespace PRNFE.MVC.Models.Response
{
    public class UserManagementResponse
    {
        public List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public int TotalCount { get; set; }
    }

    public class UserInfo
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? Dob { get; set; }
        public string WardId { get; set; } = string.Empty;
        public bool FlagActive { get; set; }
        public bool FlagAdmin { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public string? GovIDCard { get; set; }
        public string? Sex { get; set; }
        public string? Ethnic { get; set; }
        public string? Religion { get; set; }
        public string? Mst { get; set; }
        public DateTime? LogUpdTime { get; set; }
        public string? LogUpdBy { get; set; }
        public bool FlagVerifyEmail { get; set; }
    }
} 