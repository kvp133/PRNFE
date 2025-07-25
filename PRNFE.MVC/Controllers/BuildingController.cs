using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PRNFE.MVC.Controllers
{
    public class BuildingController : BaseController
    {
        public BuildingController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // GET: Building/Index
        public async Task<IActionResult> Index(FilterBuildingRequest filter)
        {
            if (!IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập lại!";
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
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"{_apiQLPTUrl}/api/Buildings/filter";
            if (!string.IsNullOrEmpty(filter.Name))
                url += $"?Name={Uri.EscapeDataString(filter.Name)}";

            try
            {
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể lấy danh sách tòa nhà: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(new List<BuildingResponse>());
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BuildingResponse>>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi lấy dữ liệu tòa nhà.";
                    TempData["IsSuccess"] = false;
                    return View(new List<BuildingResponse>());
                }

                return View(apiResponse.data ?? new List<BuildingResponse>());
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(new List<BuildingResponse>());
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(new List<BuildingResponse>());
            }
        }

        // GET: Building/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập lại!";
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
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var url = $"{_apiQLPTUrl}/api/Buildings/{id}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể lấy thông tin tòa nhà: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<BuildingResponse>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi xử lý dữ liệu tòa nhà.";
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

        // GET: Building/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            return View(new CreateBuildingRequest
            {
                Rooms = new List<CreateRoomRequest>(),
                Services = new List<CreateServiceRequest>()
            });
        }

        // POST: Building/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBuildingRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập lại!";
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
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = JsonConvert.SerializeObject(model);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var url = $"{_apiQLPTUrl}/api/Buildings";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.PostAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Tạo tòa nhà thất bại: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(model);
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<BuildingResponse>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi tạo tòa nhà.";
                    TempData["IsSuccess"] = false;
                    return View(model);
                }

                TempData["Message"] = "Tạo tòa nhà thành công!";
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

        // GET: Building/Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id) // Thêm tham số id
        {
            if (!IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập lại!";
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
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var url = $"{_apiQLPTUrl}/api/Buildings/{id}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể lấy thông tin tòa nhà: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<BuildingResponse>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi lấy dữ liệu tòa nhà.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction(nameof(Index));
                }

                var model = new UpdateBuildingRequest
                {
                    Name = apiResponse.data.Name,
                    Address = apiResponse.data.Address,
                    Description = apiResponse.data.Description,
                    NumberOfFloors = apiResponse.data.NumberOfFloors,
                    NumberOfApartments = apiResponse.data.NumberOfApartments,
                    IsActive = apiResponse.data.IsActive
                };

                return View(model);
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

        // POST: Building/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBuildingRequest model) // Thêm tham số id
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập lại!";
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
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = JsonConvert.SerializeObject(model);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var url = $"{_apiQLPTUrl}/api/Buildings/{id}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.PutAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Cập nhật tòa nhà thất bại: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(model);
                }

                TempData["Message"] = "Cập nhật tòa nhà thành công!";
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

        // GET: Building/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập lại!";
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
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var url = $"{_apiQLPTUrl}/api/Buildings/{id}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.DeleteAsync(url);

                TempData["Message"] = response.IsSuccessStatusCode
                    ? "Xóa tòa nhà thành công!"
                    : response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Không có quyền truy cập. Vui lòng đăng nhập lại!"
                        : $"Xóa tòa nhà thất bại: {await response.Content.ReadAsStringAsync()}";
                TempData["IsSuccess"] = response.IsSuccessStatusCode;

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Auth");
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}