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
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        
        public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            try
            {
                // Lấy danh sách tỉnh/thành phố
                var provincesResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Location/provinces");
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Location/districts/{provinceCode}");
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Location/wards/{districtCode}");
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
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/User/register", content);
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
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/User/forgot-password", content);
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
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/User/reset-password", content);
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
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/User/verify-email", content);
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
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/User/register", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmailApi([FromBody] VerifyEmailRequest model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/User/verify-email", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordApi([FromBody] ForgotPasswordRequest model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/User/forgot-password", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordApi([FromBody] ResetPasswordRequest model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/User/reset-password", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }
    }
} 