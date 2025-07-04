using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using PRNFE.MVC.Attributes;

namespace PRNFE.MVC.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration) 
            : base(httpClientFactory, configuration)
        {
        }

        public IActionResult Index()
        {
            var userInfo = GetUserInfo();
            
            if (userInfo == null)
            {
                return View();
            }

            // Route based on user role
            if (userInfo.IsAdmin)
            {
                // Admin users go to user management
                return RedirectToAction("UserManagement", "Admin");
            }
            else if (userInfo.IsLandlord)
            {
                // Landlords go to dormitory management
                return RedirectToAction("DormitoryManagement", "Landlord");
            }
            else if (userInfo.IsTenant)
            {
                // Tenants go to invoice information
                return RedirectToAction("InvoiceInfo", "Tenant");
            }

            // Default fallback
            return View();
        }
    }
} 