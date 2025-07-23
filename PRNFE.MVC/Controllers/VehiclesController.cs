using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Text;
using System.Text.Json;

namespace PRNFE.MVC.Controllers
{
    public class VehicleController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VehicleController> _logger;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public VehicleController(HttpClient httpClient, IConfiguration configuration, ILogger<VehicleController> logger)
        {
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

        public async Task<IActionResult> Index(VehicleFilterRequests filter)
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

            if (filter.ResidentIds?.Any() == true)
            {
                foreach (var residentId in filter.ResidentIds)
                {
                    queryParams.Add($"ResidentIds={residentId}");
                }
            }

            if (filter.RoomIds?.Any() == true)
            {
                foreach (var roomId in filter.RoomIds)
                {
                    queryParams.Add($"RoomIds={roomId}");
                }
            }

            if (filter.Type.HasValue)
                queryParams.Add($"Type={filter.Type.Value}");

            if (!string.IsNullOrWhiteSpace(filter.LicensePlate))
                queryParams.Add($"LicensePlate={Uri.EscapeDataString(filter.LicensePlate)}");

            try
            {
                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var fullUrl = $"{_apiBaseUrl}/api/Vehicles/filters{queryString}";
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
                    ViewBag.Vehicles = new List<VehicleResponses>();
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
                    ViewBag.Vehicles = new List<VehicleResponses>();
                    ViewBag.Filter = filter;
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalPages = 0;
                    ViewBag.CurrentPage = 1;
                    return View();
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<VehicleResponses>>>(json, _jsonOptions);
                var vehicles = apiResponse?.data ?? new List<VehicleResponses>();

                var totalCount = vehicles.Count;
                var paginatedVehicles = vehicles
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                ViewBag.Vehicles = paginatedVehicles;
                ViewBag.Filter = filter;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);
                ViewBag.CurrentPage = filter.Page;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách phương tiện");
                ViewBag.Error = $"Error loading vehicles: {ex.Message}";
                ViewBag.Vehicles = new List<VehicleResponses>();
                ViewBag.Filter = filter ?? new VehicleFilterRequests();
                ViewBag.TotalCount = 0;
                ViewBag.TotalPages = 0;
                ViewBag.CurrentPage = 1;
                return View();
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Vehicles/{id}");
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<DetailedVehicleResponses>>(json, _jsonOptions);
                var vehicle = apiResponse?.data;

                if (vehicle == null)
                {
                    return NotFound();
                }

                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết phương tiện cho ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết phương tiện.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadCreateFormData();
                var vehicleRequest = new VehicleCreateDtos();
                return View(vehicleRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang tạo phương tiện");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang tạo phương tiện.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleCreateDtos vehicleRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var buildingId = GetBuildingIdFromCookie();
                    var json = JsonSerializer.Serialize(vehicleRequest, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/api/Vehicles")
                    {
                        Content = content
                    };
                    request.Headers.Add("Cookie", $"buildingId={buildingId}");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<VehicleResponses>>(jsonResponse, _jsonOptions);

                    TempData["SuccessMessage"] = "Phương tiện đã được tạo thành công.";
                    return RedirectToAction(nameof(Details), new { id = apiResponse?.data?.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo phương tiện");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo phương tiện. Vui lòng thử lại.");
                }
            }

            await LoadCreateFormData();
            return View(vehicleRequest);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Vehicles/{id}");
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var vehicleResponse = await _httpClient.SendAsync(request);

                if (!vehicleResponse.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var vehicleJson = await vehicleResponse.Content.ReadAsStringAsync();
                var vehicleApiResponse = JsonSerializer.Deserialize<ApiResponse<DetailedVehicleResponses>>(vehicleJson, _jsonOptions);
                var vehicle = vehicleApiResponse?.data;

                if (vehicle == null)
                {
                    return NotFound();
                }

                var updateRequest = new VehicleUpdateDtos
                {
                    Type = vehicle.Type,
                    LicensePlate = vehicle.LicensePlate
                };

                await LoadEditFormData();
                ViewBag.VehicleId = id;
                ViewBag.Resident = vehicle.Resident;

                return View(updateRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chỉnh sửa phương tiện cho ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang chỉnh sửa phương tiện.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleUpdateDtos updateRequest)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var buildingId = GetBuildingIdFromCookie();
                    var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/api/Vehicles/{id}")
                    {
                        Content = content
                    };
                    request.Headers.Add("Cookie", $"buildingId={buildingId}");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    TempData["SuccessMessage"] = "Phương tiện đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật phương tiện cho ID: {Id}", id);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật phương tiện. Vui lòng thử lại.");
                }
            }

            await LoadEditFormData();
            ViewBag.VehicleId = id;
            return View(updateRequest);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/api/Vehicles/{id}");
                var buildingId = GetBuildingIdFromCookie();
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Phương tiện đã được xóa thành công.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Không thể xóa phương tiện: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phương tiện cho ID: {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa phương tiện.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCreateFormData()
        {
            try
            {
                var buildingId = GetBuildingIdFromCookie();

                // Load residents for dropdown
                var residentsRequest = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Residents");
                residentsRequest.Headers.Add("Cookie", $"buildingId={buildingId}");
                var residentsResponse = await _httpClient.SendAsync(residentsRequest);

                var residents = new List<ResidentListResponses>();

                if (residentsResponse.IsSuccessStatusCode)
                {
                    var residentsJson = await residentsResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation("Residents API Response: {JsonResponse}", residentsJson);
                    var residentsApiResponse = JsonSerializer.Deserialize<ApiResponse<List<ResidentListResponses>>>(residentsJson, _jsonOptions);
                    residents = residentsApiResponse?.data ?? new List<ResidentListResponses>();
                }
                else
                {
                    var errorContent = await residentsResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Residents API Error: StatusCode={StatusCode}, Content={ErrorContent}", residentsResponse.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Không thể tải danh sách cư dân. Vui lòng thử lại.";
                }

                ViewBag.Residents = residents;
                ViewBag.VehicleTypes = GetVehicleTypes();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form data: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu form. Vui lòng thử lại.";
                ViewBag.Residents = new List<ResidentListResponses>();
                ViewBag.VehicleTypes = GetVehicleTypes();
            }
        }

        private async Task LoadEditFormData()
        {
            try
            {
                ViewBag.VehicleTypes = GetVehicleTypes();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu cho form chỉnh sửa phương tiện");
                ViewBag.VehicleTypes = GetVehicleTypes();
            }
        }

        private Dictionary<int, string> GetVehicleTypes()
        {
            return new Dictionary<int, string>
            {
                { 0, "Xe máy" },
                { 1, "Ô tô" },
                { 2, "Xe đạp" },
                { 3, "Xe tải" },
                { 4, "Xe buýt" },
                { 5, "Khác" }
            };
        }

        private (string BadgeClass, string Icon, string Text) GetVehicleTypeInfo(int type)
        {
            return type switch
            {
                0 => ("bg-primary", "fas fa-motorcycle", "Xe máy"),
                1 => ("bg-success", "fas fa-car", "Ô tô"),
                2 => ("bg-info", "fas fa-bicycle", "Xe đạp"),
                3 => ("bg-warning", "fas fa-truck", "Xe tải"),
                4 => ("bg-secondary", "fas fa-bus", "Xe buýt"),
                5 => ("bg-dark", "fas fa-question", "Khác"),
                _ => ("bg-light text-dark", "fas fa-question-circle", "Không xác định")
            };
        }
    }
}
