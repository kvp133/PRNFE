using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;

namespace PRNFE.MVC.Controllers
{
    public class NotificationsController : BaseController
    {
        public NotificationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        public async Task<IActionResult> Index(FilterNotificationRequest filter)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            var buildingId = GetBuildingId();
            using var httpClient = CreateHttpClientWithCookies();

            string url = $"{_apiQLPTUrl}/api/Notifications";

            if (HasFilter(filter))
            {
                url += "/filter";

                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.Title))
                    queryParams.Add($"Title={Uri.EscapeDataString(filter.Title)}");
                if (!string.IsNullOrEmpty(filter.Content))
                    queryParams.Add($"Content={Uri.EscapeDataString(filter.Content)}");
                if (filter.TypeTarget.HasValue)
                    queryParams.Add($"TypeTarget={filter.TypeTarget.Value}");
                if (filter.PublishDate.HasValue)
                    queryParams.Add($"PublishDate={filter.PublishDate.Value:yyyy-MM-dd}");
                if (filter.FromDate.HasValue)
                    queryParams.Add($"FromDate={filter.FromDate.Value:yyyy-MM-dd}");
                if (filter.ToDate.HasValue)
                    queryParams.Add($"ToDate={filter.ToDate.Value:yyyy-MM-dd}");
                if (filter.Status.HasValue)
                    queryParams.Add($"Status={filter.Status.Value}");

                if (queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams);
                }
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("buildingId", buildingId);

            var response = await httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<NotificationResponse>>>(json);

            if (!response.IsSuccessStatusCode || apiResponse == null || !apiResponse.success)
            {
                TempData["Message"] = apiResponse?.message ?? "Lỗi khi tải danh sách thông báo.";
                TempData["IsSuccess"] = false;
                return View(new List<NotificationResponse>());
            }

            ViewBag.Filter = filter;
            return View(apiResponse.data);
        }

        private bool HasFilter(FilterNotificationRequest filter)
        {
            return filter != null &&
                (!string.IsNullOrEmpty(filter.Title) || !string.IsNullOrEmpty(filter.Content) ||
                 filter.TypeTarget.HasValue || filter.PublishDate.HasValue ||
                 filter.FromDate.HasValue || filter.ToDate.HasValue || filter.Status.HasValue);
        }

        // GET: Notifications/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiQLPTUrl}/api/Notifications/{id}");
            request.Headers.Add("buildingId", GetBuildingId());

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailsNotificationResponse>>(json);

            if (apiResponse == null || !apiResponse.success || apiResponse.data == null)
                return NotFound();

            return View(apiResponse.data);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            return View(new CreateNotificationRequest { PublishDate = DateTime.Today });
        }

        // POST: Notifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotificationRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiQLPTUrl}/api/Notifications")
            {
                Content = content
            };
            request.Headers.Add("buildingId", GetBuildingId());

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Tạo thông báo thất bại.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Notifications/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiQLPTUrl}/api/Notifications/{id}");
            request.Headers.Add("buildingId", GetBuildingId());

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailsNotificationResponse>>(json);

            if (apiResponse == null || !apiResponse.success || apiResponse.data == null)
                return NotFound();

            // Map DetailsNotificationResponse => UpdateNotificationRequest
            var notification = apiResponse.data;
            var model = new UpdateNotificationRequest
            {
                Title = notification.Title,
                Content = notification.Content,
                TypeTarget = notification.TypeTarget,
                PublishDate = notification.PublishDate,
                Status = notification.Status,
               
                ResidentIds = notification.NotificationTargets?
    .Select(nt => nt.ResidentId)
    .Where(id => id > 0)
    .ToList()

            };

            return View(model);
        }

        // POST: Notifications/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateNotificationRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiQLPTUrl}/api/Notifications/{id}")
            {
                Content = content
            };
            request.Headers.Add("buildingId", GetBuildingId());

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Cập nhật thông báo thất bại.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Notifications/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiQLPTUrl}/api/Notifications/{id}");
            request.Headers.Add("buildingId", GetBuildingId());

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<DetailsNotificationResponse>>(json);

            if (apiResponse == null || !apiResponse.success || apiResponse.data == null)
                return NotFound();

            return View(apiResponse.data);
        }

        // POST: Notifications/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Login", "Auth");
            }

            using var httpClient = CreateHttpClientWithCookies();
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiQLPTUrl}/api/Notifications/{id}");
            request.Headers.Add("buildingId", GetBuildingId());

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Xóa thông báo thất bại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
