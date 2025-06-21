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
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        
        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
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
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Auth/login", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(result);
                    if (loginResponse.success && loginResponse.data?.accessToken != null)
                    {
                        // Lưu token vào session
                        HttpContext.Session.SetString("AccessToken", loginResponse.data.accessToken);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.Message = loginResponse.message ?? "Đăng nhập thất bại!";
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

        [HttpPost]
        public async Task<IActionResult> LoginApi([FromBody] LoginRequest model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Auth/login", content);
            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }
    }
} 