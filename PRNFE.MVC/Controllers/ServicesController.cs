using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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

            using var httpClient = CreateHttpClientWithCookies();

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

            var response = await httpClient.GetAsync($"{_apiQLPTUrl}/api/Services/filter{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể lấy danh sách dịch vụ từ API.";
                TempData["IsSuccess"] = false;
                return View(new List<ServiceResponse>());
            }

            var json = await response.Content.ReadAsStringAsync();
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

        [HttpGet]
        public IActionResult Create()
        {
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
            using var httpClient = CreateHttpClientWithCookies();

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_apiQLPTUrl}/api/Services", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Tạo dịch vụ thất bại.";
                TempData["IsSuccess"] = false;
                return View(model);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();

            var response = await httpClient.GetAsync($"{_apiQLPTUrl}/api/Services/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể lấy thông tin dịch vụ từ API.";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailsServiceResponse>>(json);
            var services = apiResponse.data;

            if (apiResponse == null || !apiResponse.success)
            {
                TempData["Message"] = apiResponse?.message ?? "Lỗi khi xử lý dữ liệu dịch vụ.";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            return View(apiResponse.data);
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

            using var httpClient = CreateHttpClientWithCookies();
            var response = await httpClient.GetAsync($"{_apiQLPTUrl}/api/Services/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể lấy thông tin dịch vụ từ API.";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
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




        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateServiceRequest model)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var httpClient = CreateHttpClientWithCookies();
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"{_apiQLPTUrl}/api/Services/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể cập nhật dịch vụ.";
                TempData["IsSuccess"] = false;
                return View(model);
            }

            TempData["Message"] = "Cập nhật dịch vụ thành công!";
            TempData["IsSuccess"] = true;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Filter(FilterServiceRequest filter)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();

            // Tạo query string từ filter model
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

            var response = await httpClient.GetAsync($"{_apiQLPTUrl}/api/Services/filter{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể lấy danh sách dịch vụ theo bộ lọc từ API.";
                TempData["IsSuccess"] = false;
                return View("Index", new List<ServiceResponse>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ServiceResponse>>>(json);

            if (apiResponse == null || !apiResponse.success)
            {
                TempData["Message"] = apiResponse?.message ?? "Lỗi khi lấy dữ liệu dịch vụ.";
                TempData["IsSuccess"] = false;
                return View("Index", new List<ServiceResponse>());
            }

            var services = apiResponse.data;

            // Bạn có thể truyền filter về view để giữ giá trị filter trên form
            ViewBag.Filter = filter;

            return View("Index", services);
        }

    }
}