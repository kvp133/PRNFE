

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
    public class ResidentController : BaseController
    {
        private readonly string _apiBaseUrl;

        public ResidentController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
            _apiBaseUrl = configuration["ApiSettings:Url_qlpt"] ?? throw new InvalidOperationException("API BaseUrl is not configured.");
        }

        // INDEX
        public async Task<IActionResult> Index(ResidentFilterRequests filter)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid filter parameters.";
                return View(new ResidentFilterResponses());
            }

            if (!ValidateBuildingId())
            {
                ViewBag.Error = "Missing buildingId in cookies.";
                return View(new ResidentFilterResponses());
            }

            try
            {
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

                var queryString = string.Join("&", queryParams);
                var fullUrl = $"{_apiBaseUrl}/api/Residents/filter?{queryString}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {fullUrl}");

                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    ViewBag.Error = "Missing or invalid access token.";
                    return View(new ResidentFilterResponses());
                }

                var response = await httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(new ResidentFilterResponses());
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");

                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.Error = "Empty JSON response from API.";
                    return View(new ResidentFilterResponses());
                }

                var result = JsonConvert.DeserializeObject<ResidentFilterResponses>(json);
                ViewBag.Filter = filter;
                return View(result ?? new ResidentFilterResponses());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading residents: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                ViewBag.Filter = filter;
                return View(new ResidentFilterResponses());
            }
        }

        // CREATE - GET
        public IActionResult Create() => View(new ResidentRequests
        {
            DateOfBirth = DateTime.Now.AddYears(-18),
            Vehicles = new List<VehicleCreateDtos>(),
            Rooms = new List<RoomCreateDtos>()
        });

        // POST: Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResidentRequests model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!ValidateBuildingId())
            {
                ModelState.AddModelError(string.Empty, "Thiếu buildingId trong cookies.");
                return View(model);
            }

            try
            {
                //model.UserId = $"user_{Guid.NewGuid().ToString("N").Substring(0, 6)}"; 

                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    ModelState.AddModelError(string.Empty, "Thiếu token xác thực.");
                    return View(model);
                }

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/Residents", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorContent);
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
            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Thiếu buildingId.";
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
                    TempData["ErrorMessage"] = "Thiếu token xác thực.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Residents/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy cư dân với ID {id}.";
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ResidentResponses>>(json);
                if (apiResponse?.success != true || apiResponse.data == null)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy cư dân với ID {id}.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new ResidentUpdateRequests
                {
                    FullName = apiResponse.data.FullName,
                    PhoneNumber = apiResponse.data.PhoneNumber,
                    Email = apiResponse.data.Email,
                    DateOfBirth = apiResponse.data.DateOfBirth,
                    Address = apiResponse.data.Address,
                    Gender = apiResponse.data.Gender,
                    Rooms = apiResponse.data.Rooms?.Select(r => new RoomCreateDtos { RoomId = r.RoomId }).ToList() ?? new List<RoomCreateDtos>(),
                    Vehicles = apiResponse.data.Vehicles?.Select(v => new VehicleUpdateDtos { Type = v.Type, LicensePlate = v.LicensePlate }).ToList() ?? new List<VehicleUpdateDtos>(),
                    TemporaryStay = apiResponse.data.TemporaryStay != null ? new TemporaryStayUpdateDtos
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
        public async Task<IActionResult> Edit(int id, ResidentUpdateRequests model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ResidentId = id;
                return View(model);
            }

            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Thiếu buildingId.";
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
                    TempData["ErrorMessage"] = "Thiếu token xác thực.";
                    return RedirectToAction(nameof(Index));
                }

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"{_apiBaseUrl}/api/Residents/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(errorContent);
                        foreach (var error in errorResponse ?? new Dictionary<string, string>())
                        {
                            ModelState.AddModelError(string.Empty, error.Value);
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, $"Lỗi từ API: {errorContent}");
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
                using var httpClient = CreateHttpClientWithCookies();
                var token = GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else if (!await RefreshTokenIfNeeded())
                {
                    TempData["ErrorMessage"] = "Thiếu token xác thực.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await httpClient.DeleteAsync($"{_apiBaseUrl}/api/Residents/{id}");
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
            if (!ValidateBuildingId())
            {
                TempData["ErrorMessage"] = "Thiếu buildingId.";
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
                    TempData["ErrorMessage"] = "Thiếu token xác thực.";
                    return RedirectToAction(nameof(Index));
                }

                var response = await httpClient.GetAsync($"{_apiBaseUrl}/api/Residents/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Không tìm thấy cư dân với ID {id}: {errorContent}";
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ResidentResponses>>(json)
                    ?? throw new InvalidOperationException($"Deserialization returned null for resident ID {id}");

                if (!apiResponse.success || apiResponse.data == null || apiResponse.data.Id != id)
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy cư dân với ID {id}.";
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