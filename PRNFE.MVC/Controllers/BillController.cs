

using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Net;
using System.Text;
using System.Text.Json;
using BillResponse = PRNFE.MVC.Models.Request.BillResponses;

namespace PRNFE.MVC.Controllers
{
    public class BillController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BillController> _logger;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public BillController(HttpClient httpClient, IConfiguration configuration, ILogger<BillController> logger)
        {
            var handler = new HttpClientHandler { UseCookies = true, CookieContainer = new CookieContainer() };
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiBaseUrl = configuration.GetSection("ApiSettings:BaseUrl").Value ?? throw new InvalidOperationException("API BaseUrl is not configured.");
        }

        private string GetBuildingIdFromCookie()
        {
            var buildingId = Request.Cookies["buildingId"];
            if (string.IsNullOrEmpty(buildingId))
            {
                throw new InvalidOperationException("buildingId cookie không tồn tại hoặc rỗng.");
            }
            return buildingId;
        }

        public async Task<IActionResult> Index(BillFilterRequests filter)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid filter parameters.";
                return View();
            }

            filter.Page = filter.Page <= 0 ? 1 : filter.Page;
            filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var queryParams = new List<string>
            {
                $"Page={filter.Page}",
                $"PageSize={filter.PageSize}"
            };

            if (filter.RoomIds?.Any() == true)
            {
                foreach (var roomId in filter.RoomIds)
                {
                    queryParams.Add($"RoomIds={roomId}");
                }
            }
            if (filter.Status.HasValue)
                queryParams.Add($"Status={filter.Status.Value}");
            if (filter.FromDate.HasValue)
                queryParams.Add($"FromDate={filter.FromDate.Value:yyyy-MM-dd}");
            if (filter.ToDate.HasValue)
                queryParams.Add($"ToDate={filter.ToDate.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                queryParams.Add($"SearchTerm={Uri.EscapeDataString(filter.SearchTerm)}");

            try
            {
                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var fullUrl = $"{_apiBaseUrl}/api/Bills/filter{queryString}";
                _logger.LogInformation("Gọi API: {Url}", fullUrl);

                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";
                    ViewBag.Bills = new List<BillResponse>();
                    ViewBag.Filter = filter;
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalPages = 0;
                    ViewBag.CurrentPage = 1;
                    return View();
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("JSON Response: {JsonResponse}", json);

                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.Error = "Empty JSON response from API.";
                    ViewBag.Bills = new List<BillResponse>();
                    ViewBag.Filter = filter;
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalPages = 0;
                    ViewBag.CurrentPage = 1;
                    return View();
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<BillResponse>>>(json, _jsonOptions);
                var bills = apiResponse?.data ?? new List<BillResponse>();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    bills = bills.Where(b =>
                        (b.Room?.RoomNumber?.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                        (b.Room?.TenantId?.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                        b.Amount.ToString().Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        b.Id.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                var totalCount = bills.Count;
                var paginatedBills = bills
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                ViewBag.Bills = paginatedBills;
                ViewBag.Filter = filter;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);
                ViewBag.CurrentPage = filter.Page;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách hóa đơn");
                ViewBag.Error = $"Error loading bills: {ex.Message}";
                ViewBag.Bills = new List<BillResponse>();
                ViewBag.Filter = filter ?? new BillFilterRequests();
                ViewBag.TotalCount = 0;
                ViewBag.TotalPages = 0;
                ViewBag.CurrentPage = 1;
                return View();
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Bills/{id}");
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<BillResponse>>(json, _jsonOptions);

                var bill = apiResponse?.data;
                if (bill == null)
                {
                    return NotFound();
                }

                return View(bill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết hóa đơn cho ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết hóa đơn.";
                return RedirectToAction(nameof(Index));
            }
        }

        //CREAET
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadCreateFormData();
                var billRequest = new BillCreateRequests
                {
                    DueDate = DateTime.Now.AddDays(30),
                    BillDetails = new List<BillDetailCreateRequests>()
                };
                return View(billRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang tạo hóa đơn");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang tạo hóa đơn.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillCreateRequests billRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var buildingId = GetBuildingIdFromCookie();
                    var json = JsonSerializer.Serialize(billRequest, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/api/Bills")
                    {
                        Content = content
                    };
                    request.Headers.Add("Cookie", $"buildingId={buildingId}");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<BillResponse>>(jsonResponse, _jsonOptions);

                    TempData["SuccessMessage"] = "Hóa đơn đã được tạo thành công.";
                    return RedirectToAction(nameof(Details), new { id = apiResponse?.data?.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo hóa đơn");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo hóa đơn. Vui lòng thử lại.");
                }
            }

            await LoadCreateFormData();
            return View(billRequest);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Bills/{id}");
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var billResponse = await _httpClient.SendAsync(request);
                var servicesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Services");

                if (!billResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var billJson = await billResponse.Content.ReadAsStringAsync();
                var billApiResponse = JsonSerializer.Deserialize<ApiResponse<BillResponse>>(billJson, _jsonOptions);

                var bill = billApiResponse?.data;
                if (bill == null)
                {
                    return NotFound();
                }

                var updateRequest = new BillUpdateRequests
                {
                    Amount = bill.Amount,
                    DueDate = bill.DueDate,
                    Status = bill.Status,
                    BillDetails = bill.BillDetails?.Select(bd => new BillDetailCreateRequests
                    {
                        ServiceId = bd.ServiceId,
                        Quantity = bd.Quantity,
                        UnitPrice = bd.UnitPrice
                    }).ToList() ?? new List<BillDetailCreateRequests>()
                };

                var services = new List<BillServiceResponses>();
                if (servicesResponse.IsSuccessStatusCode)
                {
                    var servicesJson = await servicesResponse.Content.ReadAsStringAsync();
                    var servicesApiResponse = JsonSerializer.Deserialize<ApiResponse<List<BillServiceResponses>>>(servicesJson, _jsonOptions);
                    services = servicesApiResponse?.data ?? new List<BillServiceResponses>();
                }

                ViewBag.Services = services;
                ViewBag.BillId = id;
                ViewBag.Room = bill.Room;

                return View(updateRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chỉnh sửa hóa đơn cho ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang chỉnh sửa hóa đơn.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, BillUpdateRequests updateRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var buildingId = GetBuildingIdFromCookie();
                    var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/api/Bills/{id}")
                    {
                        Content = content
                    };
                    request.Headers.Add("Cookie", $"buildingId={buildingId}");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    TempData["SuccessMessage"] = "Hóa đơn đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật hóa đơn cho ID: {Id}", id);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật hóa đơn. Vui lòng thử lại.");
                }
            }

            await LoadEditFormData(id);
            ViewBag.BillId = id;
            return View(updateRequest);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/api/Bills/{id}");
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Hóa đơn đã được xóa thành công.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Không thể xóa hóa đơn: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hóa đơn cho ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa hóa đơn.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForBuilding()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/api/Bills/Building");
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<BillResponse>>>(jsonResponse, _jsonOptions);

                var createdBills = apiResponse?.data ?? new List<BillResponse>();
                TempData["SuccessMessage"] = $"Đã tạo thành công {createdBills.Count} hóa đơn cho toàn bộ tòa nhà.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hóa đơn cho toàn bộ tòa nhà");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo hóa đơn cho tòa nhà.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForRooms([FromBody] List<int> roomIds)
        {
            try
            {
                if (roomIds == null || !roomIds.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một phòng." });
                }

                var json = JsonSerializer.Serialize(roomIds, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/api/Bills/Rooms")
                {
                    Content = content
                };
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<BillResponse>>>(jsonResponse, _jsonOptions);

                var createdBills = apiResponse?.data ?? new List<BillResponse>();

                return Json(new { success = true, message = $"Đã tạo thành công {createdBills.Count} hóa đơn." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hóa đơn cho các phòng");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo hóa đơn cho các phòng." });
            }
        }

    

            private async Task LoadCreateFormData()
        {
            try
            {
                var buildingId = GetBuildingIdFromCookie();
                var roomsRequest = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Rooms");
                roomsRequest.Headers.Add("buildingId", buildingId);
                var roomsResponse = await _httpClient.SendAsync(roomsRequest);
                var servicesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Services");

                var rooms = new List<BillRoomResponses>();
                var services = new List<BillServiceResponses>();

                if (roomsResponse.IsSuccessStatusCode)
                {
                    var roomsJson = await roomsResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation("Rooms API Response: {JsonResponse}", roomsJson);
                    var roomsApiResponse = JsonSerializer.Deserialize<ApiResponse<List<BillRoomResponses>>>(roomsJson, _jsonOptions);
                    rooms = roomsApiResponse?.data ?? new List<BillRoomResponses>();
                }
                else
                {
                    var errorContent = await roomsResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Rooms API Error: StatusCode={StatusCode}, Content={ErrorContent}", roomsResponse.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Không thể tải danh sách phòng. Vui lòng thử lại.";
                }

                if (servicesResponse.IsSuccessStatusCode)
                {
                    var servicesJson = await servicesResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation("Services API Response: {JsonResponse}", servicesJson);
                    var servicesApiResponse = JsonSerializer.Deserialize<ApiResponse<List<BillServiceResponses>>>(servicesJson, _jsonOptions);
                    services = servicesApiResponse?.data ?? new List<BillServiceResponses>();
                }
                else
                {
                    var errorContent = await servicesResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Services API Error: StatusCode={StatusCode}, Content={ErrorContent}", servicesResponse.StatusCode, errorContent);
                    TempData["ErrorMessage"] = TempData["ErrorMessage"]?.ToString() + " Không thể tải danh sách dịch vụ.";
                }

                ViewBag.Rooms = rooms;
                ViewBag.Services = services;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form data: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu form. Vui lòng thử lại.";
                ViewBag.Rooms = new List<BillRoomResponses>();
                ViewBag.Services = new List<BillServiceResponses>();
            }
        }

        private async Task LoadEditFormData(string billId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Bills/{billId}");
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var billResponse = await _httpClient.SendAsync(request);
                var servicesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Services");

                var services = new List<BillServiceResponses>();
                BillRoomResponses room = null;

                if (billResponse.IsSuccessStatusCode)
                {
                    var billJson = await billResponse.Content.ReadAsStringAsync();
                    var billApiResponse = JsonSerializer.Deserialize<ApiResponse<BillResponse>>(billJson, _jsonOptions);
                    room = billApiResponse?.data?.Room;
                }

                if (servicesResponse.IsSuccessStatusCode)
                {
                    var servicesJson = await servicesResponse.Content.ReadAsStringAsync();
                    var servicesApiResponse = JsonSerializer.Deserialize<ApiResponse<List<BillServiceResponses>>>(servicesJson, _jsonOptions);
                    services = servicesApiResponse?.data ?? new List<BillServiceResponses>();
                }

                ViewBag.Services = services;
                ViewBag.Room = room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu cho form chỉnh sửa hóa đơn cho ID: {Id}", billId);
                ViewBag.Services = new List<BillServiceResponses>();
                ViewBag.Room = null;
            }
        }
    }
}
