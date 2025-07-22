using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Text.Json;

namespace PRNFE.MVC.Controllers
{
    public class ServiceController : Controller
    {
        private readonly HttpClient _httpClient;

        public ServiceController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Manager()
        {
            return View(new ServiceRequests());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manager(ServiceRequests model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Services", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.IsSuccess = true;
                    ViewBag.Message = "Thêm dịch vụ thành công!";
                    return View(new ServiceRequests());
                }
                else
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Có lỗi xảy ra khi thêm dịch vụ. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = $"Lỗi: {ex.Message}";
            }

            return View(model);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] ServiceRequests request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var service = new ServiceResponses
                {
                    ServiceId = Guid.NewGuid(),
                    ServiceName = request.ServiceName,
                    Unit = request.Unit,
                    PricePerUnit = request.PricePerUnit,
                    IsMandatory = request.IsMandatory,
                    IsPerResident = request.IsPerResident,
                    CreatedAt = DateTime.UtcNow
                };

                return Ok(service);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi thêm dịch vụ", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            // TODO: Implement get services from database
            return Ok(new List<ServiceResponses>());
        }
    }
}
