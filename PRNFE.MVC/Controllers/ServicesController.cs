
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PRNFE.MVC.Controllers
{
    [Route("[controller]/[action]")]
    public class ServicesController : BaseController
    {
        public ServicesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] FilterServiceRequest filter)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAuthenticated())
            {
                TempData["Message"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token) && !await RefreshTokenIfNeeded())
            {
                TempData["Message"] = "Không thể làm mới token. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            httpClient.DefaultRequestHeaders.Remove("buildingId");
            httpClient.DefaultRequestHeaders.Add("buildingId", GetBuildingId());
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                queryParams.Add($"Name={System.Net.WebUtility.UrlEncode(filter.Name)}");
            }
            if (filter.IsMandatory.HasValue)
            {
                queryParams.Add($"IsMandatory={filter.IsMandatory.Value.ToString().ToLower()}");
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"{_apiQLPTUrl}/api/Services/filter{queryString}";

            try
            {
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    TempData["Message"] = $"Không thể lấy danh sách dịch vụ từ API: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(new List<ServiceResponse>());
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ServiceResponse>>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi lấy dữ liệu dịch vụ.";
                    TempData["IsSuccess"] = false;
                    return View(new List<ServiceResponse>());
                }

                var services = apiResponse.data;
                ViewBag.Filter = filter;
                return View(services);
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(new List<ServiceResponse>());
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(new List<ServiceResponse>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!ValidateBuildingId() || !IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập và chọn tòa nhà trước khi tạo dịch vụ!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAuthenticated())
            {
                TempData["Message"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token) && !await RefreshTokenIfNeeded())
            {
                TempData["Message"] = "Không thể làm mới token. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            httpClient.DefaultRequestHeaders.Remove("buildingId");
            httpClient.DefaultRequestHeaders.Add("buildingId", GetBuildingId());
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var url = $"{_apiQLPTUrl}/api/Services?buildingId={GetBuildingId()}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    TempData["Message"] = $"Tạo dịch vụ thất bại: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(model);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {responseContent}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ServiceResponse>>(responseContent);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi tạo dịch vụ.";
                    TempData["IsSuccess"] = false;
                    return View(model);
                }

                TempData["Message"] = "Tạo dịch vụ thành công!";
                TempData["IsSuccess"] = true;
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(model);
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAuthenticated())
            {
                TempData["Message"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token) && !await RefreshTokenIfNeeded())
            {
                TempData["Message"] = "Không thể làm mới token. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            httpClient.DefaultRequestHeaders.Remove("buildingId");
            httpClient.DefaultRequestHeaders.Add("buildingId", GetBuildingId());
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var url = $"{_apiQLPTUrl}/api/Services/{id}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    TempData["Message"] = $"Không thể lấy thông tin dịch vụ từ API: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailsServiceResponse>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi xử lý dữ liệu dịch vụ.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction(nameof(Index));
                }

                return View(apiResponse.data);
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return RedirectToAction(nameof(Index));
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAuthenticated())
            {
                TempData["Message"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token) && !await RefreshTokenIfNeeded())
            {
                TempData["Message"] = "Không thể làm mới token. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            httpClient.DefaultRequestHeaders.Remove("buildingId");
            httpClient.DefaultRequestHeaders.Add("buildingId", GetBuildingId());
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var url = $"{_apiQLPTUrl}/api/Services/{id}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    TempData["Message"] = $"Không thể lấy thông tin dịch vụ từ API: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailsServiceResponse>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi xử lý dữ liệu dịch vụ.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction(nameof(Index));
                }

                var data = apiResponse.data;
                var updateModel = new UpdateServiceRequest
                {
                    Name = data.Name,
                    Unit = data.Unit,
                    PricePerUnit = data.PricePerUnit,
                    IsMandatory = data.IsMandatory,
                    IsActive = data.IsActive,
                    RoomServices = data.Rooms?.Select(r => new RoomInServiceRequest
                    {
                        RoomId = r.RoomId,
                        CustomPrice = r.CustomPrice
                    }).ToList() ?? new List<RoomInServiceRequest>()
                };

                ViewBag.ServiceId = id;
                return View(updateModel);
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return RedirectToAction(nameof(Index));
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateServiceRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAuthenticated())
            {
                TempData["Message"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token) && !await RefreshTokenIfNeeded())
            {
                TempData["Message"] = "Không thể làm mới token. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            httpClient.DefaultRequestHeaders.Remove("buildingId");
            httpClient.DefaultRequestHeaders.Add("buildingId", GetBuildingId());
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var url = $"{_apiQLPTUrl}/api/Services/{id}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.PutAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    TempData["Message"] = $"Không thể cập nhật dịch vụ: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(model);
                }

                TempData["Message"] = "Cập nhật dịch vụ thành công!";
                TempData["IsSuccess"] = true;
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(model);
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Filter([FromQuery] FilterServiceRequest filter)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAuthenticated())
            {
                TempData["Message"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token) && !await RefreshTokenIfNeeded())
            {
                TempData["Message"] = "Không thể làm mới token. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            httpClient.DefaultRequestHeaders.Remove("buildingId");
            httpClient.DefaultRequestHeaders.Add("buildingId", GetBuildingId());
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                queryParams.Add($"Name={System.Net.WebUtility.UrlEncode(filter.Name)}");
            }
            if (filter.IsMandatory.HasValue)
            {
                queryParams.Add($"IsMandatory={filter.IsMandatory.Value.ToString().ToLower()}");
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"{_apiQLPTUrl}/api/Services/filter{queryString}";

            try
            {
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    TempData["Message"] = $"Không thể lấy danh sách dịch vụ theo bộ lọc từ API: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View("Index", new List<ServiceResponse>());
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ServiceResponse>>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi lấy dữ liệu dịch vụ.";
                    TempData["IsSuccess"] = false;
                    return View("Index", new List<ServiceResponse>());
                }

                var services = apiResponse.data;
                ViewBag.Filter = filter;
                return View("Index", services);
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View("Index", new List<ServiceResponse>());
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View("Index", new List<ServiceResponse>());
            }
        }
    }
}