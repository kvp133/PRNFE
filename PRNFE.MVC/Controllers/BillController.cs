


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using BillResponse = PRNFE.MVC.Models.Request.BillResponses;

namespace PRNFE.MVC.Controllers
{
    public class BillController : BaseController
    {
        private readonly string _apiBaseUrl;
        private readonly ILogger<BillController> _logger;

        public BillController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<BillController> logger)
            : base(httpClientFactory, configuration)
        {
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("API BaseUrl is not configured.");
            _logger = logger;
        }

        // INDEX
        public async Task<IActionResult> Index(BillFilterRequests filter)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid filter parameters.";
                return View();
            }

            if (!ValidateBuildingId())
            {
                ViewBag.Error = "Missing buildingId in cookies.";
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

                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    ViewBag.Error = "Missing or invalid access token.";
                    return View();
                }

                var response = await httpClient.GetAsync(fullUrl);

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

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BillResponse>>>(json);
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

        // DETAILS
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    TempData["ErrorMessage"] = "Missing or invalid access token.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Bills/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<BillResponse>>(json);
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

        // CREATE - GET
        public async Task<IActionResult> Create()
        {
            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                return RedirectToAction(nameof(Index));
            }

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

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillCreateRequests billRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateFormData();
                return View(billRequest);
            }

            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                await LoadCreateFormData();
                return View(billRequest);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    ModelState.AddModelError("", "Missing or invalid access token.");
                    await LoadCreateFormData();
                    return View(billRequest);
                }

                var json = JsonConvert.SerializeObject(billRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/Bills", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo hóa đơn: {errorContent}");
                    await LoadCreateFormData();
                    return View(billRequest);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<BillResponse>>(jsonResponse);

                TempData["SuccessMessage"] = "Hóa đơn đã được tạo thành công.";
                return RedirectToAction(nameof(Details), new { id = apiResponse?.data?.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hóa đơn");
                ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo hóa đơn: {ex.Message}");
                await LoadCreateFormData();
                return View(billRequest);
            }
        }

        // EDIT - GET
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    TempData["ErrorMessage"] = "Missing or invalid access token.";
                    return RedirectToAction(nameof(Index));
                }

                var billResponse = await httpClient.GetAsync($"{_apiBaseUrl}/api/Bills/{id}");
                var servicesResponse = await httpClient.GetAsync($"{_apiBaseUrl}/api/Services");

                if (!billResponse.IsSuccessStatusCode)
                {
                    var errorContent = await billResponse.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", billResponse.StatusCode, errorContent);
                    return NotFound();
                }

                var billJson = await billResponse.Content.ReadAsStringAsync();
                var billApiResponse = JsonConvert.DeserializeObject<ApiResponse<BillResponse>>(billJson);
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
                    var servicesApiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BillServiceResponses>>>(servicesJson);
                    services = servicesApiResponse?.data ?? new List<BillServiceResponses>();
                }
                else
                {
                    var errorContent = await servicesResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Services API Error: StatusCode={StatusCode}, Content={ErrorContent}", servicesResponse.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Không thể tải danh sách dịch vụ.";
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

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, BillUpdateRequests updateRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadEditFormData(id);
                ViewBag.BillId = id;
                return View(updateRequest);
            }

            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                await LoadEditFormData(id);
                ViewBag.BillId = id;
                return View(updateRequest);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    ModelState.AddModelError("", "Missing or invalid access token.");
                    await LoadEditFormData(id);
                    ViewBag.BillId = id;
                    return View(updateRequest);
                }

                var json = JsonConvert.SerializeObject(updateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"{_apiBaseUrl}/api/Bills/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật hóa đơn: {errorContent}");
                    await LoadEditFormData(id);
                    ViewBag.BillId = id;
                    return View(updateRequest);
                }

                TempData["SuccessMessage"] = "Hóa đơn đã được cập nhật thành công.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hóa đơn cho ID: {Id}", id);
                ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật hóa đơn: {ex.Message}");
                await LoadEditFormData(id);
                ViewBag.BillId = id;
                return View(updateRequest);
            }
        }

        // DELETE
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    TempData["ErrorMessage"] = "Missing or invalid access token.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/api/Bills/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = $"Không thể xóa hóa đơn: {errorContent}";
                }
                else
                {
                    TempData["SuccessMessage"] = "Hóa đơn đã được xóa thành công.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hóa đơn cho ID: {Id}", id);
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xóa hóa đơn: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // CREATE FOR BUILDING
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForBuilding()
        {
            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    TempData["ErrorMessage"] = "Missing or invalid access token.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/Bills/Building", null);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = $"Không thể tạo hóa đơn cho tòa nhà: {errorContent}";
                    return RedirectToAction(nameof(Index));
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BillResponse>>>(jsonResponse);
                var createdBills = apiResponse?.data ?? new List<BillResponse>();

                TempData["SuccessMessage"] = $"Đã tạo thành công {createdBills.Count} hóa đơn cho toàn bộ tòa nhà.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hóa đơn cho toàn bộ tòa nhà");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tạo hóa đơn cho tòa nhà: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // CREATE FOR ROOMS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForRooms([FromBody] List<int> roomIds)
        {
            if (!ValidateBuildingId())
            {
                return Json(new { success = false, message = "Missing buildingId in cookies." });
            }

            try
            {
                if (roomIds == null || !roomIds.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một phòng." });
                }

                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    return Json(new { success = false, message = "Missing or invalid access token." });
                }

                var json = JsonConvert.SerializeObject(roomIds);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/Bills/Rooms", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    return Json(new { success = false, message = $"Không thể tạo hóa đơn cho các phòng: {errorContent}" });
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BillResponse>>>(jsonResponse);
                var createdBills = apiResponse?.data ?? new List<BillResponse>();

                return Json(new { success = true, message = $"Đã tạo thành công {createdBills.Count} hóa đơn." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hóa đơn cho các phòng");
                return Json(new { success = false, message = $"Có lỗi xảy ra khi tạo hóa đơn cho các phòng: {ex.Message}" });
            }
        }

        private async Task LoadCreateFormData()
        {
            try
            {
                if (!ValidateBuildingId())
                {
                    throw new InvalidOperationException("Missing buildingId in cookies.");
                }

                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    throw new InvalidOperationException("Missing or invalid access token.");
                }

                var roomsResponse = await httpClient.GetAsync($"{_apiBaseUrl}/api/Rooms");
                var servicesResponse = await httpClient.GetAsync($"{_apiBaseUrl}/api/Services");

                var rooms = new List<BillRoomResponses>();
                var services = new List<BillServiceResponses>();

                if (roomsResponse.IsSuccessStatusCode)
                {
                    var roomsJson = await roomsResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation("Rooms API Response: {JsonResponse}", roomsJson);
                    var roomsApiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BillRoomResponses>>>(roomsJson);
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
                    var servicesApiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BillServiceResponses>>>(servicesJson);
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
                if (!ValidateBuildingId())
                {
                    throw new InvalidOperationException("Missing buildingId in cookies.");
                }

                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    throw new InvalidOperationException("Missing or invalid access token.");
                }

                var billResponse = await httpClient.GetAsync($"{_apiBaseUrl}/api/Bills/{billId}");
                var servicesResponse = await httpClient.GetAsync($"{_apiBaseUrl}/api/Services");

                var services = new List<BillServiceResponses>();
                BillRoomResponses room = null;

                if (billResponse.IsSuccessStatusCode)
                {
                    var billJson = await billResponse.Content.ReadAsStringAsync();
                    var billApiResponse = JsonConvert.DeserializeObject<ApiResponse<BillResponse>>(billJson);
                    room = billApiResponse?.data?.Room;
                }
                else
                {
                    var errorContent = await billResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Bill API Error: StatusCode={StatusCode}, Content={ErrorContent}", billResponse.StatusCode, errorContent);
                }

                if (servicesResponse.IsSuccessStatusCode)
                {
                    var servicesJson = await servicesResponse.Content.ReadAsStringAsync();
                    var servicesApiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BillServiceResponses>>>(servicesJson);
                    services = servicesApiResponse?.data ?? new List<BillServiceResponses>();
                }
                else
                {
                    var errorContent = await servicesResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Services API Error: StatusCode={StatusCode}, Content={ErrorContent}", servicesResponse.StatusCode, errorContent);
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