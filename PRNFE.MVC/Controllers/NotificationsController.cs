using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PRNFE.MVC.Helper;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;

namespace PRNFE.MVC.Controllers
{
    public class NotificationsController : BaseController
    {
        public NotificationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(httpClientFactory, configuration)
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cookieValue = Request.Cookies["AccessToken"];
            if (JwtTokenHelper.IsTokenExpired(cookieValue))
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Auth", action = "Login" })
);
                return;
            }
            if (!JwtTokenHelper.IsLandlord(cookieValue))
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Home", action = "Index" })
);
                return;
            }
            base.OnActionExecuting(context);
        }



        // GET: NotificationsController
        public async Task<ActionResult> Index()
        {
            // You can fetch notifications from an API or database here
            // For now, we will just return an empty view
            try
            {
                var notifications = await _httpClient
                .GetFromJsonAsync<ApiResponse<List<NotificationResponse>>>($"{_apiQLPTUrl}/api/notifications");
                if (notifications != null && notifications.data.Count == 0 )
                {
                    notifications.data = new List<NotificationResponse>();
                }
                return View(notifications!.data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating HttpClient: {ex.Message}");
                TempData["Message"] = "Không thể tải thông báo. Vui lòng thử lại sau.";
                return View(new List<NotificationResponse>());
            }
        }


        
        public async Task<ActionResult> Filter(FilterNotificationRequest model)
        {
            // You can fetch notifications from an API or database here
            // For now, we will just return an empty view
            try
            {
                ViewBag.FilterModel = model;
                var apiUrl = $"{_apiQLPTUrl}/api/Notifications/filter";
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(model.Title))
                {
                    queryParams.Add($"Title={model.Title}");
                }                
                if (!string.IsNullOrEmpty(model.Content))
                {
                    queryParams.Add($"Content={model.Content}");
                }
                if (model.TypeTarget.HasValue)
                {
                    queryParams.Add($"TypeTarget={model.TypeTarget.Value}");
                }                
                if (model.PublishDate.HasValue)
                {
                    queryParams.Add($"PublishDate={model.PublishDate.Value}");
                }                
                if (model.FromDate.HasValue)
                {
                    queryParams.Add($"FromDate={model.FromDate.Value}");
                }                
                if (model.ToDate.HasValue)
                {
                    queryParams.Add($"ToDate={model.ToDate.Value}");
                }
                if (model.Status.HasValue)
                {
                    queryParams.Add($"status={model.Status.Value}");
                }
                if (queryParams.Count > 0)
                {
                    apiUrl += "?" + string.Join("&", queryParams);
                }
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<NotificationResponse>>>(apiUrl);
                if (response != null && !response.success) {
                    TempData["Message"] = response.message;
                    return View("Index", new List<NotificationResponse>());
                }
                return View("Index", response.data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating HttpClient: {ex.Message}");
                TempData["Message"] = "Không thể tải thông báo. Vui lòng thử lại sau.";
                return View("Index",new List<NotificationResponse>());
            }
        }



        // GET: NotificationsController/Details/5
        [Route("Notifications/Details/{id:guid}")]
        public async Task<ActionResult> Details(Guid id)
        {
            try
            {
                var notifications = await _httpClient
                .GetFromJsonAsync<ApiResponse<DetailsNotificationResponse>>($"{_apiQLPTUrl}/api/notifications/{id}");
                if (notifications == null)
                {
                    notifications.data = new DetailsNotificationResponse();
                }
                return View(notifications!.data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating HttpClient: {ex.Message}");
                TempData["Message"] = "Không thể tải thông báo. Vui lòng thử lại sau.";
                return View(new DetailsNotificationResponse());
            }
        }

        // GET: NotificationsController/Create
        [HttpGet]
        [Route("Notifications/Create")]
        public async Task<ActionResult> Create()
        {
            // get all room from api
            await setDataForCreate();
            return View();
        }

        private async Task setDataForCreate()
        {
            var rooms = await _httpClient.GetFromJsonAsync<ApiResponse<List<RoomResponse>>>($"{_apiQLPTUrl}/api/rooms");
            if (rooms == null)
            {
                TempData["Message"] = "Không thể tải room";
            }
            ViewBag.Rooms = rooms.data.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RoomNumber.ToString()
            })
                .ToList();
            var residents = await _httpClient.GetFromJsonAsync<ApiResponse<List<ResidentDto>>>($"{_apiQLPTUrl}/api/residents");
            if (residents == null)
            {
                TempData["Message"] = "Không thể tải resident";
            }
            ViewBag.Residents = residents.data.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r?.FullName?.ToString()
            });
        }

        // POST: NotificationsController/Create
        [HttpPost]
        [Route("Notifications/Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateNotificationRequest collection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await setDataForCreate();
                    return View(collection);
                }
                
                var apiUrl = $"{ _apiQLPTUrl}/api/notifications";
                var content = new StringContent(JsonConvert.SerializeObject(collection), 
                    Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Error creating notification: {errorResponse}");
                    await setDataForCreate();
                    return View(collection);
                }
                TempData["IsSuccess"] = true;
                TempData["Message"] = "Notification created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await setDataForCreate();
                return View(collection);
            }
        }

        // GET: NotificationsController/Edit/5
        public async Task<ActionResult> Edit(Guid id)
        {
            var notifications = await _httpClient
                .GetFromJsonAsync<ApiResponse<DetailsNotificationResponse>>($"{_apiQLPTUrl}/api/notifications/{id}");
            if (notifications == null)
            {
                notifications.data = new DetailsNotificationResponse();
            }
            var updateNoti = new UpdateNotificationRequest()
            {
                Title = notifications.data.Title,
                Content = notifications.data.Content,
                TypeTarget = notifications.data.TypeTarget,
                PublishDate = notifications.data.PublishDate,
                Status = notifications.data.Status
            };
            return View(updateNoti);
        }

        // POST: NotificationsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, UpdateNotificationRequest collection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(collection);
                }
                var apiUrl = $"{_apiQLPTUrl}/api/notifications/{id}";
                var content = new StringContent(JsonConvert.SerializeObject(collection),
                    Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = errorResponse;
                    return View(collection);
                }
                TempData["IsSuccess"] = true;
                TempData["Message"] = "Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

     
        // POST: NotificationsController/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(Guid id, IFormCollection collection)
        {
            try
            {
                var apiUrl = $"{_apiQLPTUrl}/api/notifications/{id}";
                var response = await _httpClient.DeleteAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Error deleting notification: {errorResponse}");
                    TempData["Message"] = errorResponse;
                    return View();
                }
                TempData["IsSuccess"] = true;
                TempData["Message"] = "Notification deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        // POST: NotificationsController/Push/5
        [HttpPost]
        public async Task<ActionResult> Push(Guid id)
        {
            try
            {
                var apiUrl = $"{_apiQLPTUrl}/api/notifications/push/{id}";
                var response = await _httpClient.PostAsync(apiUrl, 
                    new StringContent(string.Empty, Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Error pushing notification: {errorResponse}");
                    TempData["Message"] = errorResponse;
                    return View();
                }
                TempData["IsSuccess"] = true;
                TempData["Message"] = "Notification pushed successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
