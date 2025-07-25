
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;

namespace PRNFE.MVC.Controllers
{
    public class VehicleController : BaseController
    {
        private readonly string _apiBaseUrl;
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<VehicleController> logger)
            : base(httpClientFactory, configuration)
        {
            _apiBaseUrl = configuration["ApiSettings:Url_qlpt"] ?? throw new InvalidOperationException("API BaseUrl is not configured.");
            _logger = logger;
        }

        // INDEX
        public async Task<IActionResult> Index(VehicleFilterRequests filter)
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

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<VehicleResponses>>>(json);
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

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
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

                var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Vehicles/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailedVehicleResponses>>(json);
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

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleCreateDtos vehicleRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateFormData();
                return View(vehicleRequest);
            }

            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                await LoadCreateFormData();
                return View(vehicleRequest);
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
                    return View(vehicleRequest);
                }

                var json = JsonConvert.SerializeObject(vehicleRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/Vehicles", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo phương tiện: {errorContent}");
                    await LoadCreateFormData();
                    return View(vehicleRequest);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<VehicleResponses>>(jsonResponse);

                TempData["SuccessMessage"] = "Phương tiện đã được tạo thành công.";
                return RedirectToAction(nameof(Details), new { id = apiResponse?.data?.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phương tiện");
                ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo phương tiện: {ex.Message}");
                await LoadCreateFormData();
                return View(vehicleRequest);
            }
        }

        // EDIT - GET
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
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

                var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Vehicles/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailedVehicleResponses>>(json);
                var vehicle = apiResponse?.data;

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

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleUpdateDtos updateRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadEditFormData();
                ViewBag.VehicleId = id;
                return View(updateRequest);
            }

            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Missing buildingId in cookies.";
                await LoadEditFormData();
                ViewBag.VehicleId = id;
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
                    await LoadEditFormData();
                    ViewBag.VehicleId = id;
                    return View(updateRequest);
                }

                var json = JsonConvert.SerializeObject(updateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"{_apiBaseUrl}/api/Vehicles/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật phương tiện: {errorContent}");
                    await LoadEditFormData();
                    ViewBag.VehicleId = id;
                    return View(updateRequest);
                }

                TempData["SuccessMessage"] = "Phương tiện đã được cập nhật thành công.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phương tiện cho ID: {Id}", id);
                ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật phương tiện: {ex.Message}");
                await LoadEditFormData();
                ViewBag.VehicleId = id;
                return View(updateRequest);
            }
        }

        // DELETE
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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

                var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/api/Vehicles/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = $"Không thể xóa phương tiện: {errorContent}";
                }
                else
                {
                    TempData["SuccessMessage"] = "Phương tiện đã được xóa thành công.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phương tiện cho ID: {Id}", id);
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xóa phương tiện: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
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

                var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Residents");

                var residents = new List<ResidentListResponses>();
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Residents API Response: {JsonResponse}", json);
                    var residentsApiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ResidentListResponses>>>(json);
                    residents = residentsApiResponse?.data ?? new List<ResidentListResponses>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Residents API Error: StatusCode={StatusCode}, Content={ErrorContent}", response.StatusCode, errorContent);
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