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
        public async Task<IActionResult> Index(int page = 1, int size = 10)
        {
            try
            {
                var apiUrl = $"{_apiBaseUrl}/api/Services?$skip={((page - 1) * size)}&$top={size}";
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
            return View(new CreateServiceRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Services", content);

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
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tạo dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Services/{id}");

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Services/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var service = JsonConvert.DeserializeObject<ServiceResponse>(json);

                    if (service != null)
                    {
                        var updateRequest = new UpdateServiceRequest
                        {
                            Name = service.Name,
                            Unit = service.Unit,
                            PricePerUnit = service.PricePerUnit,
                            IsMandatory = service.IsMandatory,
                            IsActive = service.IsActive
                        };

                        ViewBag.ServiceId = id;
                        return View(updateRequest);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ServiceId = id;
                return View(request);
            }

            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/api/Services/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Cập nhật dịch vụ thành công!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể cập nhật dịch vụ: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi cập nhật dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.ServiceId = id;
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/Services/{id}");

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
                // Build query parameters
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(request.Name))
                {
                    queryParams.Add($"name={Uri.EscapeDataString(request.Name)}");
                }

                if (request.IsMandatory.HasValue)
                {
                    queryParams.Add($"isMandatory={request.IsMandatory.Value}");
                }

                if (request.IsActive.HasValue)
                {
                    queryParams.Add($"isActive={request.IsActive.Value}");
                }

                queryParams.Add($"pageSize={request.PageSize}");

                var queryString = string.Join("&", queryParams);
                var apiUrl = $"{_apiBaseUrl}/api/Services/filter?{queryString}";

                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var services = JsonConvert.DeserializeObject<List<ServiceResponse>>(json);

                    ViewBag.FilteredServices = services ?? new List<ServiceResponse>();
                    ViewBag.FilterApplied = true;
                    TempData["Message"] = $"Tìm thấy {services?.Count ?? 0} dịch vụ phù hợp!";
                    TempData["IsSuccess"] = true;
                }
                else
                {
                    ViewBag.FilteredServices = new List<ServiceResponse>();
                    ViewBag.FilterApplied = true;
                    TempData["Message"] = "Không tìm thấy dịch vụ nào phù hợp!";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                ViewBag.FilteredServices = new List<ServiceResponse>();
                ViewBag.FilterApplied = true;
                TempData["Message"] = "Có lỗi xảy ra khi lọc dịch vụ!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Services/{id}/toggle-status", null);

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
                var apiUrl = $"{_apiBaseUrl}/api/Services?{queryString}";

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
    }
}