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
    public class AuthController : BaseController
    {
        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Redirect if already logged in
            if (IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Dữ liệu không hợp lệ!";
                TempData["IsSuccess"] = false;
                return View(model);
            }

            try
            {
                var apiUrl = $"{_apiBaseUrl}/users/api/Auth/login";
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(result);
                    if (apiResponse.success)
                    {
                        // Store tokens in cookies
                        var accessTokenOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddHours(1) // 1 hour expiry
                        };

                        var refreshTokenOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(7) // 7 days expiry
                        };

                        Response.Cookies.Append("AccessToken", apiResponse.data.accessToken, accessTokenOptions);
                        Response.Cookies.Append("RefreshToken", apiResponse.data.refreshToken, refreshTokenOptions);

                        // Parse JWT token to determine redirect based on role
                        try
                        {
                            var tokenParts = apiResponse.data.accessToken.Split('.');
                            if (tokenParts.Length == 3)
                            {
                                var payload = tokenParts[1];
                                var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
                                var bytes = System.Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
                                var json = System.Text.Encoding.UTF8.GetString(bytes);
                                var tokenData = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.JwtTokenModel>(json);

                                // Redirect based on role
                                if (tokenData.IsAdmin)
                                {
                                    return RedirectToAction("UserManagement", "Admin");
                                }
                                else if (tokenData.IsLandlord)
                                {
                                    return RedirectToAction("DormitoryManagement", "Landlord");
                                }
                                else if (tokenData.IsTenant)
                                {
                                    return RedirectToAction("InvoiceInfo", "Tenant");
                                }
                            }
                        }
                        catch
                        {
                            // If token parsing fails, redirect to home
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["Message"] = apiResponse.message ?? "Đăng nhập thất bại!";
                        TempData["IsSuccess"] = false;
                    }
                }
                else
                {
                    TempData["Message"] = $"Có lỗi xảy ra (HTTP {response.StatusCode}), vui lòng thử lại sau!";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (HttpRequestException)
            {
                TempData["Message"] = "Không thể kết nối đến API server. Vui lòng kiểm tra kết nối!";
                TempData["IsSuccess"] = false;
            }
            catch (Exception)
            {
                TempData["Message"] = "Có lỗi xảy ra, vui lòng thử lại sau!";
                TempData["IsSuccess"] = false;
            }

            return View(model);
        }

        [HttpPost]
        public new IActionResult Logout()
        {
            base.Logout();
            return RedirectToAction("Login");
        }
    }
}