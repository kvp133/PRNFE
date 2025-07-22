using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;

namespace PRNFE.MVC.Controllers
{
    public class BuildingController : BaseController
    {
        public BuildingController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // GET: Building
        public async Task<IActionResult> Index([FromQuery] FilterBuildingRequest filter)
        {
            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var ownerId = GetOwnerId();

            using var httpClient = CreateHttpClientWithCookies();

            httpClient.DefaultRequestHeaders.Remove("ownerId");
            httpClient.DefaultRequestHeaders.Add("ownerId", ownerId);

            // Build query string cho filter
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(filter.Name))
                queryParams.Add($"Name={Uri.EscapeDataString(filter.Name ?? "")}");

            var query = string.Join("&", queryParams);
            var url = $"{_apiQLPTUrl}/api/Buildings/filter";
            if (!string.IsNullOrEmpty(query))
                url += "?" + query;

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                TempData["IsSuccess"] = false;
                return View(new List<BuildingResponse>());
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể lấy danh sách tòa nhà từ API.";
                TempData["IsSuccess"] = false;
                return View(new List<BuildingResponse>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BuildingResponse>>>(json);

            if (apiResponse == null || !apiResponse.success)
            {
                TempData["Message"] = apiResponse?.message ?? "Lỗi khi lấy dữ liệu tòa nhà.";
                TempData["IsSuccess"] = false;
                return View(new List<BuildingResponse>());
            }

            return View(apiResponse.data);
        }

        // GET: Building/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();

            var response = await httpClient.GetAsync($"{_apiQLPTUrl}/api/Buildings/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể lấy thông tin tòa nhà từ API.";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<BuildingResponse>>(json);

            if (apiResponse == null || !apiResponse.success)
            {
                TempData["Message"] = apiResponse?.message ?? "Lỗi khi xử lý dữ liệu tòa nhà.";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            return View(apiResponse.data);
        }

        // GET: Building/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Nếu muốn khởi tạo sẵn room/service trống để thêm từ View có thể thêm:
            var model = new CreateBuildingRequest
            {
                Rooms = new List<CreateRoomRequest>(),
                Services = new List<CreateServiceRequest>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBuildingRequest model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Dữ liệu nhập chưa hợp lệ.";
                TempData["IsSuccess"] = false;
                return View(model);
            }

            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var ownerId = GetOwnerId();

            using var httpClient = CreateHttpClientWithCookies();

            var jsonContent = JsonConvert.SerializeObject(model);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Remove("ownerId");
            httpClient.DefaultRequestHeaders.Add("ownerId", ownerId);

            var response = await httpClient.PostAsync($"{_apiQLPTUrl}/api/Buildings", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Message"] = $"Tạo tòa nhà thất bại: {error}";
                TempData["IsSuccess"] = false;
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();
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


        // GET: Building/Edit/5
        public async Task<IActionResult> Edit()
        {
            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var buildingIdString = Request.Cookies["buildingId"];
            if (!int.TryParse(buildingIdString, out var buildingId))
            {
                TempData["Message"] = "Không lấy được thông tin Tòa nhà từ cookie.";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            using var httpClient = CreateHttpClientWithCookies();

            var response = await httpClient.GetAsync($"{_apiQLPTUrl}/api/Buildings/{buildingId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể lấy thông tin tòa nhà từ API.";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
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
                // Bạn có thể map thêm Rooms, Services nếu cần
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateBuildingRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            // Lấy buildingId từ cookie
            var buildingIdString = Request.Cookies["buildingId"];
            if (!int.TryParse(buildingIdString, out var buildingId))
            {
                TempData["Message"] = "Không lấy được thông tin Tòa nhà từ cookie.";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            using var httpClient = CreateHttpClientWithCookies();

            var jsonContent = JsonConvert.SerializeObject(model);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"{_apiQLPTUrl}/api/Buildings/{buildingId}", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["Message"] = !string.IsNullOrEmpty(errorContent)
                    ? $"Cập nhật tòa nhà thất bại: {errorContent}"
                    : "Cập nhật tòa nhà thất bại, vui lòng thử lại.";

                TempData["IsSuccess"] = false;
                return View(model);
            }

            TempData["Message"] = "Cập nhật tòa nhà thành công!";
            TempData["IsSuccess"] = true;

            return RedirectToAction(nameof(Index));
        }


        // GET: Building/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();

            var response = await httpClient.DeleteAsync($"{_apiQLPTUrl}/api/Buildings/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Xóa tòa nhà thất bại, vui lòng thử lại.";
                TempData["IsSuccess"] = false;
            }
            else
            {
                TempData["Message"] = "Xóa tòa nhà thành công!";
                TempData["IsSuccess"] = true;
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper methods giả định trong BaseController hoặc bạn có thể implement
        private bool ValidateOwnerId()
        {
            // Kiểm tra cookie hoặc session chứa OwnerId
            var ownerId = GetOwnerId();
            return !string.IsNullOrEmpty(ownerId);
        }

        private string GetOwnerId()
        {
            // Lấy OwnerId từ cookie hoặc session (cách bạn lưu)
            return HttpContext.Request.Cookies["ownerId"] ?? "";
        }
    }
}
