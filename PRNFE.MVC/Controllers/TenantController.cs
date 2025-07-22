using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Attributes;

namespace PRNFE.MVC.Controllers
{
    [RequireTenant]
    public class TenantController : BaseController
    {
        public TenantController(IHttpClientFactory httpClientFactory, IConfiguration configuration) 
            : base(httpClientFactory, configuration)
        {
        }

        // GET: Tenant/InvoiceInfo
        public IActionResult InvoiceInfo()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View();
        }

        // GET: Tenant/MyRoom
        public IActionResult MyRoom()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View();
        }

        // GET: Tenant/PaymentHistory
        public IActionResult PaymentHistory()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View();
        }

        // GET: Tenant/Profile
        public IActionResult Profile()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View();
        }

        // GET: Tenant/Requests
        public IActionResult Requests()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View();
        }
    }
} 