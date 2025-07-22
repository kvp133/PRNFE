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
    public class ServicesController : BaseController
    {
        public ServicesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int size = 30)
        {
            try
            {
                var apiUrl = $"{_apiQLPTUrl}/api/Services?$skip={((page - 1) * size)}&$top={size}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var services = JsonConvert.DeserializeObject<List<ServiceResponse>>(json);

                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = size;
                    ViewBag.Services = services ?? new List<ServiceResponse>();

                    return View(services);
                }
                else
                {
                    TempData["Message"] = "Không thể tải danh sách dịch vụ!";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tải danh sách dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View(new List<ServiceResponse>());
        }


        [HttpGet]
        public IActionResult Create()
        {
            // Check if buildingId exists in cookies
            var buildingId = Request.Cookies["buildingId"];
            if (string.IsNullOrEmpty(buildingId))
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            ViewBag.BuildingId = buildingId;
            return View(new CreateServiceRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                var buildingId = Request.Cookies["buildingId"];
                ViewBag.BuildingId = buildingId;
                return View(request);
            }

            try
            {
                // Get buildingId from cookies
                var buildingId = Request.Cookies["buildingId"];
                if (string.IsNullOrEmpty(buildingId))
                {
                    TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    ViewBag.BuildingId = buildingId;
                    return View(request);
                }

                // Create HttpClient with cookies
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_apiQLPTUrl);

                // Add cookies to the request
                httpClient.DefaultRequestHeaders.Add("Cookie", $"buildingId={buildingId}");

                // Alternative: Add cookie header manually
                var cookieHeader = string.Join("; ", Request.Cookies.Select(c => $"{c.Key}={c.Value}"));
                httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/api/Services", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Tạo dịch vụ thành công!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể tạo dịch vụ: {errorContent}";
                    TempData["IsSuccess"] = false;

                    // Log for debugging
                    Console.WriteLine($"API Error: {errorContent}");
                    Console.WriteLine($"BuildingId from cookie: {buildingId}");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tạo dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            var buildingIdForView = Request.Cookies["buildingId"];
            ViewBag.BuildingId = buildingIdForView;
            return View(request);
        }

        // Alternative approach: Override the base HTTP client configuration
        private async Task<HttpResponseMessage> SendRequestWithCookies(HttpMethod method, string endpoint, object data = null)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_apiQLPTUrl);

            // Add all cookies from the current request
            var cookieHeader = string.Join("; ", Request.Cookies.Select(c => $"{c.Key}={c.Value}"));
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            HttpContent content = null;
            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var request = new HttpRequestMessage(method, endpoint)
            {
                Content = content
            };

            return await httpClient.SendAsync(request);
        }

        // Updated Create method using the helper
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWithHelper(CreateServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                var buildingId = Request.Cookies["buildingId"];
                ViewBag.BuildingId = buildingId;
                return View("Create", request);
            }

            try
            {
                // Check buildingId exists
                var buildingId = Request.Cookies["buildingId"];
                if (string.IsNullOrEmpty(buildingId))
                {
                    TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    ViewBag.BuildingId = buildingId;
                    return View("Create", request);
                }

                Console.WriteLine($"Creating service with buildingId: {buildingId}");
                Console.WriteLine($"Request data: {JsonConvert.SerializeObject(request)}");

                var response = await SendRequestWithCookies(HttpMethod.Post, "/api/Services", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Tạo dịch vụ thành công!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể tạo dịch vụ: {errorContent}";
                    TempData["IsSuccess"] = false;

                    Console.WriteLine($"API Error Response: {errorContent}");
                    Console.WriteLine($"Response Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tạo dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            var buildingIdForView = Request.Cookies["buildingId"];
            ViewBag.BuildingId = buildingIdForView;
            return View("Create", request);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiQLPTUrl}/api/Services/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var service = JsonConvert.DeserializeObject<DetailServiceResponse>(json);

                    if (service != null)
                    {
                        return View(service);
                    }
                }

                TempData["Message"] = "Không tìm thấy dịch vụ!";
                TempData["IsSuccess"] = false;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tải thông tin dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        //[HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    try
        //    {
        //        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Services/{id}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var json = await response.Content.ReadAsStringAsync();
        //            var service = JsonConvert.DeserializeObject<DetailServiceResponse>(json);

        //            if (service != null)
        //            {
        //                var updateRequest = new UpdateServiceRequest
        //                {
        //                    Name = service.Name,
        //                    Unit = service.Unit,
        //                    PricePerUnit = service.PricePerUnit,
        //                    IsMandatory = service.IsMandatory,
        //                    IsActive = service.IsActive,
        //                    RoomServices = service.RoomServices?.Select(rs => new RoomInServiceRequest
        //                    {
        //                        RoomId = rs.RoomId,
        //                        CustomPrice = rs.CustomPrice,
        //                        Room = rs.Room != null ? new RoomDetailRequest
        //                        {
        //                            Id = rs.Room.Id,
        //                            TenantId = rs.Room.TenantId,
        //                            RoomNumber = rs.Room.RoomNumber,
        //                            Floor = rs.Room.Floor,
        //                            Area = rs.Room.Area,
        //                            RoomTypeId = rs.Room.RoomTypeId,
        //                            MaxOpt = rs.Room.MaxOpt,
        //                            Status = rs.Room.Status,
        //                            CreateAt = rs.Room.CreateAt,
        //                            UpdatedAt = rs.Room.UpdatedAt
        //                        } : null
        //                    }).ToList() ?? new List<RoomInServiceRequest>()
        //                };

        //                ViewBag.ServiceId = id;
        //                ViewBag.AvailableRooms = await GetAvailableRooms();
        //                return View(updateRequest);
        //            }
        //        }

        //        TempData["Message"] = "Không tìm thấy dịch vụ!";
        //        TempData["IsSuccess"] = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Message"] = "Có lỗi xảy ra khi tải thông tin dịch vụ!";
        //        TempData["IsSuccess"] = false;
        //        Console.WriteLine($"Error: {ex.Message}");
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, UpdateServiceRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.ServiceId = id;
        //        ViewBag.AvailableRooms = await GetAvailableRooms();
        //        return View(request);
        //    }

        //    try
        //    {
        //        // Transform to match API expected format exactly
        //        var apiRequest = new
        //        {
        //            name = request.Name,
        //            unit = request.Unit,
        //            pricePerUnit = request.PricePerUnit,
        //            isMandatory = request.IsMandatory,
        //            isActive = request.IsActive,
        //            roomServices = request.RoomServices.Select(rs => new
        //            {
        //                roomId = rs.RoomId,
        //                customPrice = rs.CustomPrice,
        //                room = rs.Room != null ? new
        //                {
        //                    id = rs.Room.Id,
        //                    tenantId = rs.Room.TenantId,
        //                    roomNumber = rs.Room.RoomNumber,
        //                    floor = rs.Room.Floor,
        //                    area = rs.Room.Area,
        //                    roomTypeId = rs.Room.RoomTypeId,
        //                    maxOpt = rs.Room.MaxOpt,
        //                    status = rs.Room.Status,
        //                    createAt = rs.Room.CreateAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
        //                    updatedAt = rs.Room.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        //                } : null
        //            }).ToArray()
        //        };

        //        var json = JsonConvert.SerializeObject(apiRequest, new JsonSerializerSettings
        //        {
        //            DateFormatHandling = DateFormatHandling.IsoDateFormat,
        //            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        //        });

        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = await _httpClient.PutAsync($"{_apiBaseUrl}/api/Services/{id}", content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            TempData["Message"] = "Cập nhật dịch vụ thành công!";
        //            TempData["IsSuccess"] = true;
        //            return RedirectToAction(nameof(Details), new { id = id });
        //        }
        //        else
        //        {
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            TempData["Message"] = $"Không thể cập nhật dịch vụ: {errorContent}";
        //            TempData["IsSuccess"] = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Message"] = "Có lỗi xảy ra khi cập nhật dịch vụ!";
        //        TempData["IsSuccess"] = false;
        //        Console.WriteLine($"Error: {ex.Message}");
        //    }

        //    ViewBag.ServiceId = id;
        //    ViewBag.AvailableRooms = await GetAvailableRooms();
        //    return View(request);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiQLPTUrl}/api/Services/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Xóa dịch vụ thành công!";
                    TempData["IsSuccess"] = true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể xóa dịch vụ: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi xóa dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Filter()
        {
            return View(new FilterServiceRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Filter(FilterServiceRequest request)
        {
            try
            {
                // Validate building ID
                if (!ValidateBuildingId())
                {
                    TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    return View(request);
                }

                // Create filter object matching API structure
                var filterDto = new
                {
                    name = request.Name ?? string.Empty,
                    isMandatory = request.IsMandatory
                    // Note: API doesn't support isActive filter based on your API structure
                };

                using var httpClient = CreateHttpClientWithCookies();

                var json = JsonConvert.SerializeObject(filterDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending filter request: {json}");
                Console.WriteLine($"BuildingId: {GetBuildingId()}");

                var response = await httpClient.PostAsync("api/Services/filter", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var services = JsonConvert.DeserializeObject<List<ServiceResponse>>(responseJson);

                    // Apply client-side filtering for IsActive since API doesn't support it
                    if (request.IsActive.HasValue)
                    {
                        services = services?.Where(s => s.IsActive == request.IsActive.Value).ToList();
                    }

                    // Apply PageSize limit
                    if (services != null && services.Count > request.PageSize)
                    {
                        services = services.Take(request.PageSize).ToList();
                    }

                    ViewBag.FilteredServices = services ?? new List<ServiceResponse>();
                    ViewBag.FilterApplied = true;
                    TempData["Message"] = $"Tìm thấy {services?.Count ?? 0} dịch vụ phù hợp!";
                    TempData["IsSuccess"] = true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.FilteredServices = new List<ServiceResponse>();
                    ViewBag.FilterApplied = true;
                    TempData["Message"] = $"Không thể lọc dịch vụ: {errorContent}";
                    TempData["IsSuccess"] = false;

                    Console.WriteLine($"API Error: {response.StatusCode}");
                    Console.WriteLine($"Error Content: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                ViewBag.FilteredServices = new List<ServiceResponse>();
                ViewBag.FilterApplied = true;
                TempData["Message"] = "Có lỗi xảy ra khi lọc dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return View(request);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_apiQLPTUrl}/api/Services/{id}/toggle-status", null);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false });
            }
        }

        // API endpoint for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetServices(int page = 1, int size = 10, string search = "")
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"$skip={((page - 1) * size)}",
                    $"$top={size}"
                };

                if (!string.IsNullOrEmpty(search))
                {
                    queryParams.Add($"$filter=contains(Name,'{Uri.EscapeDataString(search)}')");
                }

                var queryString = string.Join("&", queryParams);
                var apiUrl = $"{_apiQLPTUrl}/api/Services?{queryString}";

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var services = JsonConvert.DeserializeObject<List<ServiceResponse>>(json);

                    return Json(new
                    {
                        success = true,
                        data = services,
                        currentPage = page,
                        pageSize = size
                    });
                }

                return Json(new { success = false, message = "Không thể tải danh sách dịch vụ" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCustomPrice([FromBody] UpdateCustomPriceRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiQLPTUrl}/api/Services/{request.ServiceId}/rooms/{request.RoomId}/custom-price", content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false });
            }
        }

        // Thêm model request
        public class UpdateCustomPriceRequest
        {
            public int ServiceId { get; set; }
            public int RoomId { get; set; }
            public decimal CustomPrice { get; set; }
        }
    }
}