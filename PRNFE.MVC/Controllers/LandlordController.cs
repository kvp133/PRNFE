using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Attributes;

namespace PRNFE.MVC.Controllers
{
    [RequireLandlord]
    public class LandlordController : BaseController
    {
        public LandlordController(IHttpClientFactory httpClientFactory, IConfiguration configuration) 
            : base(httpClientFactory, configuration)
        {
        }

        // GET: Landlord/DormitoryManagement
        public IActionResult DormitoryManagement()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View();
        }

        // GET: Landlord/ServiceManagement
        public IActionResult ServiceManagement()
        {
            var buildingId = Request.Cookies["BuildingId"];
            if (string.IsNullOrEmpty(buildingId))
                return RedirectToAction("SelectBuilding", "Auth");
            ViewBag.BuildingId = buildingId;
            return View();
        }

        // GET: Landlord/RoomManagement
        public IActionResult RoomManagement()
        {
            var buildingId = Request.Cookies["BuildingId"];
            if (string.IsNullOrEmpty(buildingId))
                return RedirectToAction("SelectBuilding", "Auth");
            ViewBag.BuildingId = buildingId;
            return View();
        }

        // GET: Landlord/TenantManagement
        public IActionResult TenantManagement()
        {
            var buildingId = Request.Cookies["BuildingId"];
            if (string.IsNullOrEmpty(buildingId))
                return RedirectToAction("SelectBuilding", "Auth");
            ViewBag.BuildingId = buildingId;
            return View();
        }

        // GET: Landlord/InvoiceManagement
        public IActionResult InvoiceManagement()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View();
        }

        // GET: Landlord/Reports
        public IActionResult Reports()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View();
        }
    }
} 