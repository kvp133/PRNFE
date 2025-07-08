
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PRNFE.MVC.Controllers
{
    public class ResidentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly string _apiBaseUrl;

        public ResidentController(HttpClient httpClient, IConfiguration configuration)
        {
            var handler = new HttpClientHandler { UseCookies = true, CookieContainer = new CookieContainer() };
            _httpClient = httpClient;
            _apiBaseUrl = configuration.GetSection("ApiSettings:BaseUrl").Value ?? throw new InvalidOperationException("API BaseUrl is not configured.");
        }

        // INDEX
        public async Task<IActionResult> Index(ResidentFilterRequest filter)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid filter parameters.";
                return View(new ResidentFilterResponse());
            }

            var queryParams = new List<string>
    {
        $"Page={filter.Page}",
        $"PageSize={filter.PageSize}"
    };

            if (!string.IsNullOrEmpty(filter.FullName))
            {
                queryParams.Add($"FullName={Uri.EscapeDataString(filter.FullName)}");
            }

            if (!string.IsNullOrEmpty(filter.PhoneNumber))
            {
                queryParams.Add($"PhoneNumber={Uri.EscapeDataString(filter.PhoneNumber)}");
            }

            if (filter.RoomIds != null && filter.RoomIds.Length > 0)
            {
                queryParams.Add($"RoomIds={string.Join(",", filter.RoomIds)}");
            }

            try
            {
                var queryString = string.Join("&", queryParams);
                var fullUrl = $"{_apiBaseUrl}/api/Residents/filter?{queryString}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {fullUrl}");

                var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                var buildingId = Request.Cookies["buildingId"];
                if (!string.IsNullOrEmpty(buildingId))
                {
                    request.Headers.Add("Cookie", $"buildingId={buildingId}");
                }
                else
                {
                    ViewBag.Error = "Missing buildingId in cookies.";
                    return View(new ResidentFilterResponse());
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(new ResidentFilterResponse());
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");

                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.Error = "Empty JSON response from API.";
                    return View(new ResidentFilterResponse());
                }

                var result = JsonSerializer.Deserialize<ResidentFilterResponse>(json, _jsonOptions);
                ViewBag.Filter = filter;
                return View(result ?? new ResidentFilterResponse());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading residents: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                ViewBag.Filter = filter;
                return View(new ResidentFilterResponse());
            }
        }


        //// CREATE - GET
        public IActionResult Create() => View(new ResidentRequest
        {
            DateOfBirth = DateTime.Now.AddYears(-18),
            Vehicles = new List<VehicleCreateDto>(),
            Rooms = new List<RoomCreateDto>()
        });

        // POST: Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResidentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Sinh userId bằng GUID để đảm bảo duy nhất
                model.UserId = $"user_{Guid.NewGuid().ToString("N").Substring(0, 6)}"; // Ví dụ: user_a1b2c3

                var buildingId = Request.Cookies["buildingId"];
                if (string.IsNullOrEmpty(buildingId))
                {
                    ModelState.AddModelError(string.Empty, "Thiếu buildingId trong cookies.");
                    return View(model);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/api/Residents")
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(model, _jsonOptions),
                        Encoding.UTF8,
                        "application/json"
                    )
                };
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent, _jsonOptions);
                        foreach (var error in errorResponse ?? new Dictionary<string, string>())
                        {
                            ModelState.AddModelError(string.Empty, error.Value);
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, $"Lỗi từ API: {errorContent}");
                    }
                    return View(model);
                }

                TempData["SuccessMessage"] = "Thêm cư dân thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
                return View(model);
            }
        }
        // EDIT - GET
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Residents/{id}");
                var buildingId = Request.Cookies["buildingId"];
                if (string.IsNullOrEmpty(buildingId))
                {
                    TempData["ErrorMessage"] = "Thiếu buildingId.";
                    return RedirectToAction(nameof(Index));
                }
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy cư dân với ID {id}.";
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ResidentResponse>>(json, _jsonOptions);
                if (apiResponse?.success != true || apiResponse.data == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy cư dân với ID {id}.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new ResidentUpdateRequest
                {
                    FullName = apiResponse.data.FullName,
                    PhoneNumber = apiResponse.data.PhoneNumber,
                    Email = apiResponse.data.Email,
                    DateOfBirth = apiResponse.data.DateOfBirth,
                    Address = apiResponse.data.Address,
                    Gender = apiResponse.data.Gender,
                    Rooms = apiResponse.data.Rooms?.Select(r => new RoomCreateDto { RoomId = r.RoomId }).ToList() ?? [],
                    Vehicles = apiResponse.data.Vehicles?.Select(v => new VehicleUpdateDto { Type = v.Type, LicensePlate = v.LicensePlate }).ToList() ?? [],
                    TemporaryStay = apiResponse.data.TemporaryStay != null ? new TemporaryStayUpdateDto
                    {
                        FromDate = apiResponse.data.TemporaryStay.FromDate,
                        ToDate = apiResponse.data.TemporaryStay.ToDate,
                        Note = apiResponse.data.TemporaryStay.Note,
                        Status = apiResponse.data.TemporaryStay.Status
                    } : null
                };

                ViewBag.ResidentId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResidentUpdateRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ResidentId = id;
                return View(model);
            }

            try
            {
                var buildingId = Request.Cookies["buildingId"];
                if (string.IsNullOrEmpty(buildingId))
                {
                    TempData["ErrorMessage"] = "Thiếu buildingId.";
                    return RedirectToAction(nameof(Index));
                }

                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/api/Residents/{id}")
                {
                    Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Cookie", $"buildingId={buildingId}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(errorContent, _jsonOptions);
                        foreach (var error in errorResponse ?? new Dictionary<string, string>())
                        {
                            ModelState.AddModelError(string.Empty, error.Value);
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi từ API: " + errorContent);
                    }

                    ViewBag.ResidentId = id;
                    return View(model);
                }

                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                ViewBag.ResidentId = id;
                return View(model);
            }
        }

        // DELETE
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/Residents/{id}");
                TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                    response.IsSuccessStatusCode ? "Resident deleted successfully!" : $"Error deleting resident: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/api/Residents/{id}");
                var buildingId = Request.Cookies["buildingId"];
               
                if (string.IsNullOrEmpty(buildingId))
                {
                    return RedirectToAction(nameof(Index));
                }
                request.Headers.Add("Cookie", $"buildingId={buildingId}");
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return RedirectToAction(nameof(Index));
                }
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ResidentResponse>>(json, _jsonOptions)
                    ?? throw new InvalidOperationException($"Deserialization returned null for resident ID {id}");
                if (!apiResponse.success)
                {
                    return RedirectToAction(nameof(Index));
                }
                if (apiResponse.data == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                if (apiResponse.data.Id != id)
                {
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.ResidentId = id;
                ViewBag.ApiStatusCode = response.StatusCode;
                return View(apiResponse.data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading resident with ID {id}: {ex.Message}. StackTrace: {ex.StackTrace}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
