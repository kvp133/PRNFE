using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Response;
using PRNFE.MVC.Models;
using Newtonsoft.Json.Linq;

namespace PRNFE.MVC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _apiBaseUrl;
        protected readonly string _apiQLPTUrl;
        protected readonly IHttpClientFactory _httpClientFactory;

        protected BaseController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {

            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
            _apiQLPTUrl = configuration["ApiSettings:Url_qlpt"];
            _httpClientFactory = httpClientFactory;

        }
        protected HttpClient CreateHttpClientWithCookies()
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_apiQLPTUrl);

            // Add all cookies from current request
            if (Request.Cookies.Any())
            {
                var cookieHeader = string.Join("; ", Request.Cookies.Select(c => $"{c.Key}={c.Value}"));
                httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);

                // Debug logging
                Console.WriteLine($"Sending cookies: {cookieHeader}");
            }

            return httpClient;
        }
        protected bool ValidateBuildingId()
        {
            var buildingId = Request.Cookies["buildingId"];
            return !string.IsNullOrEmpty(buildingId);
        }

        protected string GetBuildingId()
        {
            return Request.Cookies["buildingId"] ?? string.Empty;
        }
        protected bool IsAuthenticated()
        {
            return Request.Cookies["AccessToken"] != null;
        }

        protected string GetAccessToken()
        {
            return Request.Cookies["AccessToken"];
        }

        protected string GetRefreshToken()
        {
            return Request.Cookies["RefreshToken"];
        }

        protected async Task<bool> RefreshTokenIfNeeded()
        {
            var refreshToken = GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            try
            {
                var refreshRequest = new { refreshToken = refreshToken };
                var apiUrl = $"{_apiBaseUrl}/api/Auth/refresh";
                var content = new StringContent(JsonConvert.SerializeObject(refreshRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(result);
                    if (apiResponse.success)
                    {
                        // Update cookies with new tokens
                        var accessTokenOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddHours(1)
                        };

                        var refreshTokenOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(7)
                        };

                        Response.Cookies.Append("AccessToken", apiResponse.data.accessToken, accessTokenOptions);
                        Response.Cookies.Append("RefreshToken", apiResponse.data.refreshToken, refreshTokenOptions);
                        return true;
                    }
                }
            }
            catch
            {
                // Silent fail
            }

            return false;
        }

        protected void Logout()
        {
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");
        }

        protected JwtTokenModel GetUserInfo()
        {
            return HttpContext.Items["UserInfo"] as JwtTokenModel;
        }

        protected (string userName, string email, string fullName) GetUserInfoFromToken()
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token)) return (null, null, null);
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return (null, null, null);
                var payload = parts[1];
                var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
                var bytes = System.Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var obj = JObject.Parse(json);
                string userName = obj["user_name"]?.ToString();
                string email = obj["email"]?.ToString();
                string fullName = obj["full_name"]?.ToString();
                return (userName, email, fullName);
            }
            catch
            {
                return (null, null, null);
            }
        }
    }
} 