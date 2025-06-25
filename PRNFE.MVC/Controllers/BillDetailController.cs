using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Text.Json;

namespace PRNFE.MVC.Controllers
{
    public class BillDetailController : Controller
    {
        private readonly HttpClient _httpClient;

        public BillDetailController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: BillDetail/Manager
        public IActionResult Manager(Guid? billId = null)
        {
            var model = new BillDetailRequest();
            if (billId.HasValue)
            {
                model.BillId = billId.Value;
            }

            ViewBag.BillId = billId;
            return View(model);
        }

        // POST: BillDetail/Manager
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manager(BillDetailRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/BillDetails", content);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.IsSuccess = true;
                    ViewBag.Message = "Thêm chi tiết hóa đơn thành công!";
                    return View(new BillDetailRequest { BillId = model.BillId });
                }
                else
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Có lỗi xảy ra khi thêm chi tiết hóa đơn. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = $"Lỗi: {ex.Message}";
            }

            return View(model);
        }

        // GET: BillDetail/List/{billId}
        public async Task<IActionResult> List(Guid billId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/BillDetails/bill/{billId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var billDetails = JsonSerializer.Deserialize<List<BillDetailResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    ViewBag.BillId = billId;
                    return View(billDetails ?? new List<BillDetailResponse>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi khi tải danh sách: {ex.Message}";
            }

            return View(new List<BillDetailResponse>());
        }
    }

    // API Controller
    [ApiController]
    [Route("api/[controller]")]
    public class BillDetailsController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateBillDetail([FromBody] BillDetailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var billDetail = new BillDetailResponse
                {
                    BillDetailId = Guid.NewGuid(),
                    BillId = request.BillId,
                    ServiceId = request.ServiceId,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice,
                    Total = request.Quantity * request.UnitPrice,
                    ServiceName = "TBD", // TODO: Get from database
                    Unit = "TBD" // TODO: Get from database
                };

                return Ok(billDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi thêm chi tiết hóa đơn", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBillDetails()
        {
            // TODO: Implement get bill details from database
            return Ok(new List<BillDetailResponse>());
        }

        [HttpGet("bill/{billId}")]
        public async Task<IActionResult> GetBillDetailsByBillId(Guid billId)
        {
            try
            {
                // TODO: Get bill details from database by billId
                var mockData = new List<BillDetailResponse>
                {
                    new BillDetailResponse
                    {
                        BillDetailId = Guid.NewGuid(),
                        BillId = billId,
                        ServiceId = Guid.NewGuid(),
                        ServiceName = "Điện",
                        Unit = "kWh",
                        Quantity = 150,
                        UnitPrice = 3500,
                        Total = 525000
                    },
                    new BillDetailResponse
                    {
                        BillDetailId = Guid.NewGuid(),
                        BillId = billId,
                        ServiceId = Guid.NewGuid(),
                        ServiceName = "Nước",
                        Unit = "m³",
                        Quantity = 8,
                        UnitPrice = 25000,
                        Total = 200000
                    }
                };

                return Ok(mockData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBillDetail(Guid id)
        {
            try
            {
                // TODO: Delete from database
                return Ok(new { message = "Xóa chi tiết hóa đơn thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa", error = ex.Message });
            }
        }
    }
}
