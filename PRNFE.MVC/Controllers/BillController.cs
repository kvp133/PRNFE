using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Text.Json;

namespace PRNFE.MVC.Controllers
{
    public class BillController : Controller
    {
        private readonly HttpClient _httpClient;

        public BillController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Manager()
        {
            return View(new BillRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manager(BillRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Bills", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.IsSuccess = true;
                    ViewBag.Message = "Tạo hóa đơn thành công!";
                    return View(new BillRequest());
                }
                else
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Có lỗi xảy ra khi tạo hóa đơn. Vui lòng thử lại.";
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
    public class BillsController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateBill([FromBody] BillRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var bill = new BillResponse
                {
                    BillId = Guid.NewGuid(),
                    RoomId = request.RoomId,
                    BillMonth = request.BillMonth,
                    TotalAmount = request.TotalAmount,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow,
                    RoomNumber = "TBD" // TODO: Get from database
                };

                return Ok(bill);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo hóa đơn", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBills()
        {
            // TODO: Implement get bills from database
            return Ok(new List<BillResponse>());
        }
    }
}
