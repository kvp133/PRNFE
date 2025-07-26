

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static PRNFE.MVC.Models.Response.DetailsRoomResponse;


namespace PRNFE.MVC.Controllers
{
    public class RoomsController : BaseController
    {
        public RoomsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        public async Task<IActionResult> Index(FilterRoomRequest filter)
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

            var buildingId = GetBuildingId();

            using var httpClient = CreateHttpClientWithCookies();
            httpClient.DefaultRequestHeaders.Remove("buildingId");
            httpClient.DefaultRequestHeaders.Add("buildingId", buildingId);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            string url;
            bool hasFilter = !string.IsNullOrEmpty(filter.RoomNumber)
                             || filter.Floor.HasValue
                             || filter.RoomType.HasValue
                             || filter.Status.HasValue;

            if (hasFilter)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.RoomNumber))
                    queryParams.Add($"RoomNumber={Uri.EscapeDataString(filter.RoomNumber)}");
                if (filter.Floor.HasValue)
                    queryParams.Add($"Floor={filter.Floor.Value}");
                if (filter.RoomType.HasValue)
                    queryParams.Add($"RoomType={filter.RoomType.Value}");
                if (filter.Status.HasValue)
                    queryParams.Add($"Status={filter.Status.Value}");

                var query = string.Join("&", queryParams);
                url = $"{_apiQLPTUrl}/api/Rooms/filter";
                if (!string.IsNullOrEmpty(query))
                    url += "?" + query;
            }
            else
            {
                url = $"{_apiQLPTUrl}/api/Rooms";
            }

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

                    TempData["Message"] = $"Không thể lấy danh sách phòng từ API: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(new List<RoomResponse>());
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RoomResponse>>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi lấy dữ liệu phòng";
                    TempData["IsSuccess"] = false;
                    return View(new List<RoomResponse>());
                }

                var rooms = apiResponse.data ?? new List<RoomResponse>();
                return View(rooms);
            }
            catch (HttpRequestException ex)
            {
                TempData["Message"] = $"Lỗi kết nối đến API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(new List<RoomResponse>());
            }
            catch (JsonException ex)
            {
                TempData["Message"] = $"Lỗi phân tích dữ liệu từ API: {ex.Message}";
                TempData["IsSuccess"] = false;
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                return View(new List<RoomResponse>());
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
                var url = $"{_apiQLPTUrl}/api/Rooms/{id}";
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

                    TempData["Message"] = $"Không thể lấy thông tin phòng từ API: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailsRoomResponse>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi xử lý dữ liệu phòng.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction(nameof(Index));
                }

                var roomDetails = apiResponse.data;
                return View(roomDetails);
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
        public IActionResult Create()
        {
            if (!ValidateBuildingId() || !IsAuthenticated())
            {
                TempData["Message"] = "Vui lòng đăng nhập và chọn tòa nhà trước khi tạo phòng!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoomRequest model)
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

            var buildingId = GetBuildingId();

            using var httpClient = CreateHttpClientWithCookies();
            httpClient.DefaultRequestHeaders.Remove("buildingId");
            httpClient.DefaultRequestHeaders.Add("buildingId", buildingId);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var jsonContent = JsonConvert.SerializeObject(model);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var url = $"{_apiQLPTUrl}/api/Rooms?buildingId={buildingId}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.PostAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    TempData["Message"] = $"Tạo phòng thất bại: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(model);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {responseJson}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RoomResponse>>(responseJson);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi tạo phòng.";
                    TempData["IsSuccess"] = false;
                    return View(model);
                }

                TempData["Message"] = "Tạo phòng thành công!";
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
        public async Task<IActionResult> Edit(int id)
        {
            if (!ValidateBuildingId() || !IsAuthenticated())
            {
                TempData["Message"] = "Phiên đăng nhập đã hết hạn hoặc không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
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
                var url = $"{_apiQLPTUrl}/api/Rooms/{id}";
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

                    TempData["Message"] = $"Không thể lấy thông tin phòng: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON Response: {json}");
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailsRoomResponse>>(json);

                if (apiResponse == null || !apiResponse.success)
                {
                    TempData["Message"] = apiResponse?.message ?? "Lỗi khi xử lý dữ liệu.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction(nameof(Index));
                }

                var room = apiResponse.data;

                var model = new UpdateRoomRequest
                {
                    TenantId = room.TenantId,
                    RoomNumber = room.RoomNumber,
                    Floor = room.Floor,
                    Area = room.Area,
                    RoomType = room.RoomType,
                    MaxOpt = room.MaxOpt,
                    Status = room.Status,
                    Residents = room.Residents?.Select(r => new UpdateResidentInRoomDto
                    {
                        ResidentId = r.ResidentId,
                        IsActive = r.IsActive

                    }).ToList() ?? new List<ResidentOption>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting residents: {ex.Message}");
            }

            return new List<ResidentOption>();
        }

        private async Task<List<ServiceOption>> GetAvailableServices()
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var response = await httpClient.GetAsync("api/Services");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var services = JsonConvert.DeserializeObject<List<ServiceResponses>>(json);

                    return services?.Where(s => s.IsActive).Select(s => new ServiceOption

                    }).ToList() ?? new List<UpdateResidentInRoomDto>(),
                    Services = room.Services?.Select(s => new UpdateServiceInRoomDto

                    {
                        ServiceId = s.ServiceId,
                        IsActive = s.IsActive,
                        CustomPrice = s.CustomPrice
                    }).ToList() ?? new List<UpdateServiceInRoomDto>()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateRoomRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!ValidateBuildingId() || !IsAuthenticated())
            {
                TempData["Message"] = "Phiên đăng nhập đã hết hạn hoặc không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
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

            var jsonContent = JsonConvert.SerializeObject(model);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var url = $"{_apiQLPTUrl}/api/Rooms/{id}";
                System.Diagnostics.Debug.WriteLine($"Request URL: {url}");
                var response = await httpClient.PutAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["Message"] = "Không có quyền truy cập. Vui lòng đăng nhập lại!";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Login", "Auth");
                    }

                    TempData["Message"] = $"Cập nhật phòng thất bại: {errorContent}";
                    TempData["IsSuccess"] = false;
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return View(model);
                }

                TempData["Message"] = "Cập nhật phòng thành công!";
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
    }
}