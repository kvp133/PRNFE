using System.Collections.Generic;

namespace PRNFE.MVC.Models
{
    public class JwtTokenModel
    {
        public string user_name { get; set; }
        public string email { get; set; }
        public string full_name { get; set; }
        public string flag_admin { get; set; }
        public string role { get; set; }
        public List<string> permission { get; set; }
        public long nbf { get; set; }
        public long exp { get; set; }
        public long iat { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }

        public bool IsAdmin => flag_admin?.ToLower() == "true";
        public bool IsLandlord => role == "Quản lý trọ";
        public bool IsTenant => role == "Người thuê trọ";
    }
} 