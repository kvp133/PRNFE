using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using Microsoft.Extensions.Configuration;

namespace PRNFE.MVC.Controllers
{
    [Route("[controller]/[action]")]
    public class BuildingsController : BaseController
    {
        public BuildingsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int size = 10, string sortBy = "Name", string sortOrder = "asc")
        {
            try
            {
                if (!ValidateOwnerId())
                {
                    TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Login", "Account");
                }

                using var httpClient = CreateHttpClientWithCookies();

                var ownerId = GetOwnerId();
                var apiUrl = $"api/Buildings?ownerId={ownerId}&$skip={((page - 1) * size)}&$top={size}&$orderby={sortBy} {sortOrder}";
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var buildings = JsonConvert.DeserializeObject<List<BuildingResponse>>(json);

                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = size;
                    ViewBag.SortBy = sortBy;
                    ViewBag.SortOrder = sortOrder;
                    ViewBag.Buildings = buildings ?? new List<BuildingResponse>();

                    return View(buildings);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể tải danh sách tòa nhà: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tải danh sách tòa nhà!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View(new List<BuildingResponse>());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var response = await httpClient.GetAsync($"api/Buildings/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var building = JsonConvert.DeserializeObject<BuildingResponse>(json);

                    if (building != null)
                    {
                        return View(building);
                    }
                }

                TempData["Message"] = "Không tìm thấy tòa nhà!";
                TempData["IsSuccess"] = false;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tải thông tin tòa nhà!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.OwnerId = GetOwnerId();
            return View(new CreateBuildingRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBuildingRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OwnerId = GetOwnerId();
                return View(request);
            }

            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return View(request);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var ownerId = GetOwnerId();
                httpClient.DefaultRequestHeaders.Add("ownerId", ownerId);

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Creating building with ownerId: {ownerId}");
                Console.WriteLine($"Request data: {json}");

                var response = await httpClient.PostAsync("api/Buildings", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Tạo tòa nhà thành công!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể tạo tòa nhà: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tạo tòa nhà!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.OwnerId = GetOwnerId();
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (!ValidateOwnerId())
                {
                    TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction(nameof(Index));
                }

                using var httpClient = CreateHttpClientWithCookies();

                var response = await httpClient.GetAsync($"api/Buildings/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var building = JsonConvert.DeserializeObject<BuildingResponse>(json);

                    if (building != null)
                    {
                        var updateRequest = new UpdateBuildingRequest
                        {
                            Name = building.Name,
                            Address = building.Address,
                            Description = building.Description,
                            NumberOfFloors = building.NumberOfFloors,
                            NumberOfApartments = building.NumberOfApartments,
                            IsActive = building.IsActive
                        };

                        ViewBag.BuildingId = id;
                        ViewBag.OwnerId = GetOwnerId();

                        return View(updateRequest);
                    }
                }

                TempData["Message"] = "Không tìm thấy tòa nhà!";
                TempData["IsSuccess"] = false;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tải thông tin tòa nhà!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBuildingRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BuildingId = id;
                ViewBag.OwnerId = GetOwnerId();
                return View(request);
            }

            if (!ValidateOwnerId())
            {
                TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                ViewBag.BuildingId = id;
                ViewBag.OwnerId = GetOwnerId();
                return View(request);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Updating building {id}");
                Console.WriteLine($"Request data: {json}");
                Console.WriteLine($"IsActive = {request.IsActive}");


                var response = await httpClient.PutAsync($"api/Buildings/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Cập nhật tòa nhà thành công!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể cập nhật tòa nhà: {errorContent}";
                    TempData["IsSuccess"] = false;
                    Console.WriteLine($"Error response: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi cập nhật tòa nhà!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.BuildingId = id;
            ViewBag.OwnerId = GetOwnerId();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!ValidateOwnerId())
                {
                    TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction(nameof(Index));
                }

                using var httpClient = CreateHttpClientWithCookies();

                var response = await httpClient.DeleteAsync($"api/Buildings/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Xóa tòa nhà thành công!";
                    TempData["IsSuccess"] = true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể xóa tòa nhà: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi xóa tòa nhà!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Filter()
        {
            return View(new FilterBuildingRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Filter(FilterBuildingRequest request)
        {
            try
            {
                if (!ValidateOwnerId())
                {
                    TempData["Message"] = "Không tìm thấy thông tin chủ sở hữu. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    return View(request);
                }

                var filterDto = new
                {
                    name = request.Name ?? string.Empty,
                    address = request.Address ?? string.Empty,
                    minFloors = request.MinFloors,
                    maxFloors = request.MaxFloors,
                    minApartments = request.MinApartments,
                    maxApartments = request.MaxApartments,
                    isActive = request.IsActive,
                    createdFrom = request.CreatedFrom?.ToString("yyyy-MM-dd"),
                    createdTo = request.CreatedTo?.ToString("yyyy-MM-dd"),
                    sortBy = request.SortBy,
                    sortOrder = request.SortOrder
                };

                using var httpClient = CreateHttpClientWithCookies();

                var ownerId = GetOwnerId();
                httpClient.DefaultRequestHeaders.Add("ownerId", ownerId);

                var json = JsonConvert.SerializeObject(filterDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending filter request: {json}");

                var response = await httpClient.PostAsync("api/Buildings/filter", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var buildings = JsonConvert.DeserializeObject<List<BuildingResponse>>(responseJson);

                    // Apply PageSize limit
                    if (buildings != null && buildings.Count > request.PageSize)
                    {
                        buildings = buildings.Take(request.PageSize).ToList();
                    }

                    ViewBag.FilteredBuildings = buildings ?? new List<BuildingResponse>();
                    ViewBag.FilterApplied = true;
                    TempData["Message"] = $"Tìm thấy {buildings?.Count ?? 0} tòa nhà phù hợp!";
                    TempData["IsSuccess"] = true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.FilteredBuildings = new List<BuildingResponse>();
                    ViewBag.FilterApplied = true;
                    TempData["Message"] = $"Không thể lọc tòa nhà: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                ViewBag.FilteredBuildings = new List<BuildingResponse>();
                ViewBag.FilterApplied = true;
                TempData["Message"] = "Có lỗi xảy ra khi lọc tòa nhà!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View(request);
        }

        // Helper methods
        private bool ValidateOwnerId()
        {
            var ownerId = HttpContext.Request.Cookies["ownerId"];
            return !string.IsNullOrEmpty(ownerId) && int.TryParse(ownerId, out _);
        }

        private string GetOwnerId()
        {
            return HttpContext.Request.Cookies["ownerId"] ?? "0";
        }

        // Helper method to get building status text
        public static string GetBuildingStatusText(bool isActive) => isActive ? "Hoạt động" : "Không hoạt động";

        public static string GetBuildingStatusColor(bool isActive) => isActive ? "success" : "danger";
    }
}
