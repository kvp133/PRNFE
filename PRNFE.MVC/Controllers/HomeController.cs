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
                    // Nếu đã ở trang tạo building thì không redirect nữa
                    if (currentController == "Building" && currentAction == "Create")
                        return View();
                    return RedirectToAction("Create", "Building");
                }
                return RedirectToAction("DormitoryManagement", "Landlord");
            }
            else if (userInfo.IsTenant)
            {
                var roomId = Request.Cookies["RoomId"];
                var buildingId = Request.Cookies["BuildingId"];
                var currentController = ControllerContext.RouteData.Values["controller"]?.ToString();
                var currentAction = ControllerContext.RouteData.Values["action"]?.ToString();
                if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(buildingId))
                {
                    // Nếu đã ở trang chọn phòng thì không redirect nữa
                    if (currentController == "Auth" && currentAction == "SelectRoom")
                        return base.View();
                    return RedirectToAction("SelectRoom", "Auth");
                }
                return RedirectToAction("ManageVehicle", "Tenant");
            }

            // Default fallback
            return View();
        }
    }
} 