using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using PRNFE.MVC.Models.Location;
using Microsoft.Extensions.Configuration;

namespace PRNFE.MVC.Controllers
{
    [Route("[controller]/[action]")]
    public class UserController : BaseController
    {
        
        public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            try
            {
                // Lấy danh sách tỉnh/thành phố
                var provincesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/users/api/Location/provinces");
                if (provincesResponse.IsSuccessStatusCode)
                {
                    var provincesJson = await provincesResponse.Content.ReadAsStringAsync();
                    var provinces = JsonConvert.DeserializeObject<List<Province>>(provincesJson);
                    ViewBag.Provinces = provinces;
                }
            }
            catch
            {
                ViewBag.Provinces = new List<Province>();
            }
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts(string provinceCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/users/api/Location/districts/{provinceCode}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var districts = JsonConvert.DeserializeObject<List<District>>(json);
                    return Json(districts);
                }
            }
            catch
            {
                // Silent fail
            }
            
            return Json(new List<District>());
        }

        [HttpGet]
        public async Task<IActionResult> GetWards(string districtCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/users/api/Location/wards/{districtCode}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var wards = JsonConvert.DeserializeObject<List<Ward>>(json);
                    return Json(wards);
                }
            }
            catch
            {
                // Silent fail
            }
            
            return Json(new List<Ward>());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Dữ liệu không hợp lệ!";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/User/register", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                    if (apiResponse.success)
                    {
                        ViewBag.Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực.";
                        ViewBag.IsSuccess = true;
                        return RedirectToAction("VerifyEmail", new { email = model.email });
                    }
                    else
                    {
                        ViewBag.Message = apiResponse.message ?? "Đăng ký thất bại!";
                        ViewBag.IsSuccess = false;
                    }
                }
                else
                {
                    // Đọc message từ response body nếu có
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                        ViewBag.Message = apiResponse.message ?? "Có lỗi xảy ra, vui lòng thử lại sau!";
                    }
                    catch
                    {
                        ViewBag.Message = "Có lỗi xảy ra, vui lòng thử lại sau!";
                    }
                    ViewBag.IsSuccess = false;
                }
            }
            catch
            {
                ViewBag.Message = "Có lỗi xảy ra, vui lòng thử lại sau!";
                ViewBag.IsSuccess = false;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Dữ liệu không hợp lệ!";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/User/forgot-password", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                    if (apiResponse.success)
                    {
                        ViewBag.Message = "Email đã được gửi! Vui lòng kiểm tra hộp thư của bạn.";
                        ViewBag.IsSuccess = true;
                        ViewBag.Email = model.Email;
                    }
                    else
                    {
                        ViewBag.Message = apiResponse.message ?? "Gửi email thất bại!";
                        ViewBag.IsSuccess = false;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra, vui lòng thử lại sau!";
                    ViewBag.IsSuccess = false;
                }
            }
            catch
            {
                ViewBag.Message = "Có lỗi xảy ra, vui lòng thử lại sau!";
                ViewBag.IsSuccess = false;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Dữ liệu không hợp lệ!";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/User/reset-password", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                    if (apiResponse.success)
                    {
                        ViewBag.Message = "Đặt lại mật khẩu thành công!";
                        ViewBag.IsSuccess = true;
                        return RedirectToAction("Login", "Auth");
                    }
                    else
                    {
                        ViewBag.Message = apiResponse.message ?? "Đặt lại mật khẩu thất bại!";
                        ViewBag.IsSuccess = false;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra, vui lòng thử lại sau!";
                    ViewBag.IsSuccess = false;
                }
            }
            catch
            {
                ViewBag.Message = "Có lỗi xảy ra, vui lòng thử lại sau!";
                ViewBag.IsSuccess = false;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Dữ liệu không hợp lệ!";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/User/verify-email", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                    if (apiResponse.success)
                    {
                        ViewBag.Message = "Xác thực email thành công! Tài khoản của bạn đã được kích hoạt.";
                        ViewBag.IsSuccess = true;
                        ViewBag.Email = model.email;
                        return View(model);
                    }
                    else
                    {
                        ViewBag.Message = apiResponse.message ?? "Xác thực email thất bại!";
                        ViewBag.IsSuccess = false;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra, vui lòng thử lại sau!";
                    ViewBag.IsSuccess = false;
                }
            }
            catch
            {
                ViewBag.Message = "Có lỗi xảy ra, vui lòng thử lại sau!";
                ViewBag.IsSuccess = false;
            }

            ViewBag.Email = model.email;
            return View(model);
        }

        // API endpoints for JavaScript calls (if needed)
        [HttpPost]
        public async Task<IActionResult> RegisterApi([FromBody] RegisterRequest model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/User/register", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmailApi([FromBody] VerifyEmailRequest model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/User/verify-email", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordApi([FromBody] ForgotPasswordRequest model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/User/forgot-password", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordApi([FromBody] ResetPasswordRequest model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/User/reset-password", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            // Gọi API lấy thông tin user hiện tại
            var apiUrl = $"{_apiBaseUrl}/users/api/User/current";
            var response = await _httpClient.GetAsync(apiUrl);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Không thể lấy thông tin người dùng.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", "Home");
            }
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<dynamic>>(result);
            if (apiResponse == null || !apiResponse.success)
            {
                TempData["Message"] = apiResponse?.message ?? "Không thể lấy thông tin người dùng.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", "Home");
            }
            // Map sang model UpdateProfileRequest nếu cần
            var model = JsonConvert.DeserializeObject<PRNFE.MVC.Models.Request.UpdateProfileRequest>(JsonConvert.SerializeObject(apiResponse.data));
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(PRNFE.MVC.Models.Request.UpdateProfileRequest model)
        {
            if (!ModelState.IsValid)
            {
                TempData["MessageUpdateProfile"] = "Dữ liệu không hợp lệ!";
                TempData["IsSuccessUpdateProfile"] = false;
                return View(model);
            }
            (string userName, _, _) = this.GetUserInfoFromToken();
            if (string.IsNullOrEmpty(userName))
            {
                TempData["MessageUpdateProfile"] = "Không tìm thấy thông tin người dùng.";
                TempData["IsSuccessUpdateProfile"] = false;
                return RedirectToAction("Login", "Auth");
            }
            var apiUrl = $"{_apiBaseUrl}/users/api/User/update?userName={userName}";
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, apiUrl) { Content = content };
            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                if (apiResponse.success)
                {
                    TempData["MessageUpdateProfile"] = "Cập nhật thông tin thành công!";
                    TempData["IsSuccessUpdateProfile"] = true;
                    return RedirectToAction("UpdateProfile");
                }
                else
                {
                    TempData["MessageUpdateProfile"] = apiResponse.message ?? "Cập nhật thất bại!";
                    TempData["IsSuccessUpdateProfile"] = false;
                }
            }
            else
            {
                try
                {
                    var apiErrorResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
                    TempData["Message"] = apiErrorResponse.message ?? "Cập nhật thất bại!";
                }
                catch
                {
                    TempData["Message"] = "Cập nhật thất bại!";
                }
                TempData["IsSuccess"] = false;
            }
            return View(model);
        }
    }
} 