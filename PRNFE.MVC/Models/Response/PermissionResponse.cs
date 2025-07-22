using System;

namespace PRNFE.MVC.Models.Response
{
    public class PermissionResponse
    {
        public string PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
        public bool FlagActive { get; set; }
        public DateTime LogCreateTime { get; set; }
    }
} 