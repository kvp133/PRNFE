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
                // Landlords: kiểm tra đã chọn building chưa
                var buildingId = Request.Cookies["BuildingId"];
                var currentController = ControllerContext.RouteData.Values["controller"]?.ToString();
                var currentAction = ControllerContext.RouteData.Values["action"]?.ToString();
                if (string.IsNullOrEmpty(buildingId))
                {
                    // Nếu đã ở trang chọn building thì không redirect nữa
                    if (currentController == "Auth" && currentAction == "SelectBuilding")
                        return View();
                    return RedirectToAction("SelectBuilding", "Auth");
                }
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