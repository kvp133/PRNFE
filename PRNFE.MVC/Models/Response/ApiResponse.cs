using System.Collections.Generic;

namespace PRNFE.MVC.Models.Response
{
    public class ApiResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public T data { get; set; }
        public List<string> errors { get; set; }
    }
} 