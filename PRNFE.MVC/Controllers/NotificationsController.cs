using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Text;

namespace PRNFE.MVC.Controllers
{
    public class NotificationsController : BaseController
    {
        public NotificationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(httpClientFactory, configuration)
        {
        }

        // GET: NotificationsController
        public async Task<ActionResult> Index()
        {
            // You can fetch notifications from an API or database here
            // For now, we will just return an empty view
            try
            {
                var notifications = await _httpClient
                .GetFromJsonAsync<ApiResponse<List<NotificationResponse>>>("/api/notifications");
                if (notifications != null && notifications.data.Count == 0 )
                {
                    notifications.data = new List<NotificationResponse>();
                }
                return View(notifications!.data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating HttpClient: {ex.Message}");
                TempData["ErrorMessage"] = "Không thể tải thông báo. Vui lòng thử lại sau.";
                return View(new List<NotificationResponse>());
            }
        }

        // GET: NotificationsController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var notifications = await _httpClient
                .GetFromJsonAsync<ApiResponse<List<DetailsNotificationResponse>>>("/api/notifications/{id}");
                if (notifications != null && notifications.data.Count == 0)
                {
                    notifications.data = new List<DetailsNotificationResponse>();
                }
                return View(notifications!.data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating HttpClient: {ex.Message}");
                TempData["ErrorMessage"] = "Không thể tải thông báo. Vui lòng thử lại sau.";
                return View(new List<NotificationResponse>());
            }
        }

        // GET: NotificationsController/Create
        public async Task<ActionResult> Create()
        {
            return View();
        }

        // POST: NotificationsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UpdateNotificationRequest collection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(collection);
                }
                var apiUrl = "/api/notifications";
                var content = new StringContent(JsonConvert.SerializeObject(collection), 
                    Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Error creating notification: {errorResponse}");
                    return View(collection);
                }
                TempData["SuccessMessage"] = "Notification created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: NotificationsController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            return View();
        }

        // POST: NotificationsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // POST: NotificationsController/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                var apiUrl = $"/api/notifications/{id}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Error deleting notification: {errorResponse}");
                    return View();
                }
                TempData["SuccessMessage"] = "Notification deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
