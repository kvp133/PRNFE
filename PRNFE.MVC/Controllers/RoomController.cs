using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Text.Json;

namespace PRNFE.MVC.Controllers
{
    public class RoomController : Controller
    {
        private readonly HttpClient _httpClient;

        public RoomController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Room/Manager
        public IActionResult Manager()
        {
            return View(new RoomRequests());
        }

        // POST: Room/Manager
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manager(RoomRequests model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Room/manager", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.IsSuccess = true;
                    ViewBag.Message = "Tạo phòng thành công!";
                    return View(new RoomRequests()); // Reset form
                }
                else
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Có lỗi xảy ra khi tạo phòng. Vui lòng thử lại.";
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

    // API Controller
    [ApiController]
    [Route("api/[controller]")]
    public class RoomApiController : ControllerBase
    {
        [HttpPost("manager")]
        public async Task<IActionResult> CreateRoom([FromBody] RoomRequests request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Here you would typically save to database
                var room = new RoomResponses
                {
                    RoomId = Guid.NewGuid(),
                    RoomNumber = request.RoomNumber,
                    Floor = request.Floor,
                    Area = request.Area,
                    RoomType = request.RoomType,
                    MaxOccupants = request.MaxOccupants,
                    CreatedAt = DateTime.UtcNow
                };

                // TODO: Save to database
                // await _roomService.CreateRoomAsync(room);

                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo phòng", error = ex.Message });
            }
        }
    }
}
