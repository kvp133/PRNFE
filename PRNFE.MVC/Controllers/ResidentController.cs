using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Text.Json;

namespace PRNFE.MVC.Controllers
{
    public class ResidentController : Controller
    {
        private readonly HttpClient _httpClient;

        public ResidentController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Manager()
        {
            return View(new ResidentRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manager(ResidentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Residents", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.IsSuccess = true;
                    ViewBag.Message = "Thêm cư dân thành công!";
                    return View(new ResidentRequest());
                }
                else
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Có lỗi xảy ra khi thêm cư dân. Vui lòng thử lại.";
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
    public class ResidentsController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateResident([FromBody] ResidentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resident = new ResidentResponse
                {
                    ResidentId = Guid.NewGuid(),
                    FullName = request.FullName,
                    IdentityNumber = request.IdentityNumber,
                    BirthDate = request.BirthDate,
                    Gender = request.Gender,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    PermanentAddress = request.PermanentAddress,
                    ResidentType = request.ResidentType,
                    RoomId = request.RoomId,
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                return Ok(resident);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi thêm cư dân", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetResidents()
        {
            // TODO: Implement get residents from database
            return Ok(new List<ResidentResponse>());
        }
    }
}
