using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using Microsoft.Extensions.Configuration;
using PRNFE.MVC.Attributes;

namespace PRNFE.MVC.Controllers
{
    [RequireAdmin]
    public class AdminController : BaseController
    {
        public AdminController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> UserManagement(int page = 1, int pageSize = 6)
        {
            try
            {
                var apiUrl = $"{_apiBaseUrl}/users/api/User/paged?page={page}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync(apiUrl);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UserManagementResponse>>(result);
                    if (apiResponse?.success == true && apiResponse.data != null)
                    {
                        ViewBag.CurrentPage = page;
                        ViewBag.PageSize = pageSize;
                        ViewBag.TotalCount = apiResponse.data.TotalCount;
                        ViewBag.TotalPages = (int)Math.Ceiling((double)apiResponse.data.TotalCount / pageSize);
                        
                        return View(apiResponse.data.Users);
                    }
                }

                TempData["MessageUserManagement"] = "Không thể lấy danh sách người dùng.";
                TempData["IsSuccessUserManagement"] = false;
                return View(new List<UserInfo>());
            }
            catch
            {
                TempData["MessageUserManagement"] = "Có lỗi xảy ra khi lấy danh sách người dùng.";
                TempData["IsSuccessUserManagement"] = false;
                return View(new List<UserInfo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                TempData["MessageUserManagement"] = "Tên người dùng không được để trống!";
                TempData["IsSuccessUserManagement"] = false;
                return RedirectToAction("UserManagement");
            }

            try
            {
                var apiUrl = $"{_apiBaseUrl}/users/api/User/lock?userName={Uri.EscapeDataString(userName)}";
                var response = await _httpClient.PostAsync(apiUrl, null);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                    if (apiResponse?.success == true)
                    {
                        TempData["MessageUserManagement"] = "Khóa người dùng thành công!";
                        TempData["IsSuccessUserManagement"] = true;
                    }
                    else
                    {
                        TempData["MessageUserManagement"] = apiResponse?.message ?? "Khóa người dùng thất bại!";
                        TempData["IsSuccessUserManagement"] = false;
                    }
                }
                else
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                        TempData["MessageUserManagement"] = apiResponse?.message ?? "Khóa người dùng thất bại!";
                    }
                    catch
                    {
                        TempData["MessageUserManagement"] = "Khóa người dùng thất bại!";
                    }
                    TempData["IsSuccessUserManagement"] = false;
                }
            }
            catch
            {
                TempData["MessageUserManagement"] = "Có lỗi xảy ra khi khóa người dùng.";
                TempData["IsSuccessUserManagement"] = false;
            }

            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                TempData["MessageUserManagement"] = "Tên người dùng không được để trống!";
                TempData["IsSuccessUserManagement"] = false;
                return RedirectToAction("UserManagement");
            }

            try
            {
                var apiUrl = $"{_apiBaseUrl}/users/api/User/unlock?userName={Uri.EscapeDataString(userName)}";
                var response = await _httpClient.PostAsync(apiUrl, null);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                    if (apiResponse?.success == true)
                    {
                        TempData["MessageUserManagement"] = "Mở khóa người dùng thành công!";
                        TempData["IsSuccessUserManagement"] = true;
                    }
                    else
                    {
                        TempData["MessageUserManagement"] = apiResponse?.message ?? "Mở khóa người dùng thất bại!";
                        TempData["IsSuccessUserManagement"] = false;
                    }
                }
                else
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                        TempData["MessageUserManagement"] = apiResponse?.message ?? "Mở khóa người dùng thất bại!";
                    }
                    catch
                    {
                        TempData["MessageUserManagement"] = "Mở khóa người dùng thất bại!";
                    }
                    TempData["IsSuccessUserManagement"] = false;
                }
            }
            catch
            {
                TempData["MessageUserManagement"] = "Có lỗi xảy ra khi mở khóa người dùng.";
                TempData["IsSuccessUserManagement"] = false;
            }

            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                TempData["MessageUserManagement"] = "Tên người dùng không được để trống!";
                TempData["IsSuccessUserManagement"] = false;
                return RedirectToAction("UserManagement");
            }

            try
            {
                var apiUrl = $"{_apiBaseUrl}/users/api/User/reset-password-auto?userNameOrEmail={Uri.EscapeDataString(userName)}";
                var response = await _httpClient.PostAsync(apiUrl, null);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                    if (apiResponse?.success == true)
                    {
                        TempData["MessageUserManagement"] = "Đặt lại mật khẩu thành công! Mật khẩu mới đã được gửi qua email.";
                        TempData["IsSuccessUserManagement"] = true;
                    }
                    else
                    {
                        TempData["MessageUserManagement"] = apiResponse?.message ?? "Đặt lại mật khẩu thất bại!";
                        TempData["IsSuccessUserManagement"] = false;
                    }
                }
                else
                {
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                        TempData["MessageUserManagement"] = apiResponse?.message ?? "Đặt lại mật khẩu thất bại!";
                    }
                    catch
                    {
                        TempData["MessageUserManagement"] = "Đặt lại mật khẩu thất bại!";
                    }
                    TempData["IsSuccessUserManagement"] = false;
                }
            }
            catch
            {
                TempData["MessageUserManagement"] = "Có lỗi xảy ra khi đặt lại mật khẩu.";
                TempData["IsSuccessUserManagement"] = false;
            }

            return RedirectToAction("UserManagement");
        }
    }
} 