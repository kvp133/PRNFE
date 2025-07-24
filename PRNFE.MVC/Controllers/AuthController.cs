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
                                    // Lấy danh sách building của landlord
                                    var buildingsApiUrl = $"{_apiBaseUrl}/residents/api/Buildings";
                                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, buildingsApiUrl);
                                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiResponse.data.accessToken);
                                    var buildingsResponse = await _httpClient.SendAsync(requestMessage);
                                    var buildingsResult = await buildingsResponse.Content.ReadAsStringAsync();
                                    var buildingsApiResponse = JsonConvert.DeserializeObject<PRNFE.MVC.Models.Response.ApiResponse<List<PRNFE.MVC.Models.Response.BuildingResponse>>>(buildingsResult);
                                    if (buildingsApiResponse.success && buildingsApiResponse.data != null)
                                    {
                                        var landlordBuildings = buildingsApiResponse.data
                                            .Where(b => b.OwnerId != null && b.OwnerId.ToString() == tokenData.user_name)
                                            .ToList();
                                        if (landlordBuildings.Count == 1)
                                        {
                                            // Lưu buildingId vào cookie
                                            var buildingIdOptions = new CookieOptions
                                            {
                                                HttpOnly = true,
                                                Secure = true,
                                                SameSite = SameSiteMode.Strict,
                                                Expires = DateTime.UtcNow.AddDays(7)
                                            };
                                            Response.Cookies.Append("BuildingId", landlordBuildings[0].Id.ToString(), buildingIdOptions);
                                            return RedirectToAction("DormitoryManagement", "Landlord");
                                        }
                                        else if (landlordBuildings.Count > 1)
                                        {
                                            // Chuyển hướng sang trang chọn building
                                            TempData["LandlordBuildings"] = JsonConvert.SerializeObject(landlordBuildings);
                                            return RedirectToAction("SelectBuilding", "Auth");
                                        }
                                        else
                                        {
                                            // Không có building nào thuộc quyền quản lý, chuyển sang trang tạo building
                                            return RedirectToAction("Create", "Building");
                                        }
                                    }
                                    else
                                    {
                                        TempData["Message"] = "Không thể lấy danh sách trọ. Vui lòng thử lại sau!";
                                        TempData["IsSuccess"] = false;
                                        return View(model);
                                    }
                                }
                                else if (tokenData.IsTenant)
                                {
                                    // Lấy danh sách phòng của tenant
                                    var roomsApiUrl = $"{_apiBaseUrl}/residents/api/Rooms/user";
                                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, roomsApiUrl);
                                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiResponse.data.accessToken);
                                    var roomsResponse = await _httpClient.SendAsync(requestMessage);
                                    var roomsResult = await roomsResponse.Content.ReadAsStringAsync();
                                    var roomsApiResponse = JsonConvert.DeserializeObject<PRNFE.MVC.Models.Response.ApiResponse<List<PRNFE.MVC.Models.Response.RoomResponse>>>(roomsResult);
                                    if (roomsApiResponse.success && roomsApiResponse.data != null)
                                    {
                                        var tenantRooms = roomsApiResponse.data;
                                        if (tenantRooms.Count == 1)
                                        {
                                            // Lưu buildingId và roomId vào cookie
                                            var buildingIdOptions = new CookieOptions
                                            {
                                                HttpOnly = true,
                                                Secure = false,
                                                SameSite = SameSiteMode.Lax,
                                                Expires = DateTime.UtcNow.AddDays(7)
                                            };
                                            Response.Cookies.Append("BuildingId", tenantRooms[0].BuildingId.ToString(), buildingIdOptions);
                                            Response.Cookies.Append("RoomId", tenantRooms[0].Id.ToString(), buildingIdOptions);
                                            return RedirectToAction("InvoiceInfo", "Tenant");
                                        }
                                        else if (tenantRooms.Count > 1)
                                        {
                                            TempData["TenantRooms"] = JsonConvert.SerializeObject(tenantRooms);
                                            return RedirectToAction("SelectRoom", "Auth");
                                        }
                                        else
                                        {
                                            TempData["Message"] = "Không tìm thấy phòng nào cho tài khoản này.";
                                            TempData["IsSuccess"] = false;
                                            return View(model);
                                        }
                                    }
                                    else
                                    {
                                        TempData["Message"] = "Không thể lấy danh sách phòng. Vui lòng thử lại sau!";
                                        TempData["IsSuccess"] = false;
                                        return View(model);
                                    }
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

        [HttpGet]
        public async Task<IActionResult> SelectBuilding()
        {
            // Nếu có TempData thì dùng, không thì tự gọi lại API lấy danh sách building
            List<PRNFE.MVC.Models.Response.BuildingResponse> buildings = null;
            if (TempData["LandlordBuildings"] != null)
            {
                var buildingsJson = TempData["LandlordBuildings"] as string;
                buildings = JsonConvert.DeserializeObject<List<PRNFE.MVC.Models.Response.BuildingResponse>>(buildingsJson);
            }
            else
            {
                // Lấy access token từ cookie
                var accessToken = Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Message"] = "Vui lòng đăng nhập lại!";
                    return RedirectToAction("Login");
                }
                var apiUrl = $"{_apiBaseUrl}/residents/api/Buildings";
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.SendAsync(requestMessage);
                var result = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<PRNFE.MVC.Models.Response.ApiResponse<List<PRNFE.MVC.Models.Response.BuildingResponse>>>(result);
                if (apiResponse.success && apiResponse.data != null)
                {
                    // Lấy user_name từ token
                    var token = accessToken;
                    var parts = token.Split('.');
                    var payload = parts[1];
                    var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
                    var bytes = System.Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
                    var json = System.Text.Encoding.UTF8.GetString(bytes);
                    var tokenData = Newtonsoft.Json.JsonConvert.DeserializeObject<PRNFE.MVC.Models.JwtTokenModel>(json);
                    buildings = apiResponse.data.Where(b => b.OwnerId != null && b.OwnerId.ToString() == tokenData.user_name).ToList();
                }
            }
            if (buildings == null || buildings.Count == 0)
            {
                TempData["Message"] = "Không tìm thấy danh sách trọ.";
                return RedirectToAction("Login");
            }
            return View(buildings);
        }

        [HttpPost]
        public IActionResult SelectBuilding(int buildingId)
        {
            var buildingIdOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Để test local, nếu dùng HTTPS thì để true
                SameSite = SameSiteMode.Lax, // Để tránh lỗi cookie không lưu trên localhost
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("BuildingId", buildingId.ToString(), buildingIdOptions);
            return RedirectToAction("DormitoryManagement", "Landlord");
        }

        [HttpGet]
        public async Task<IActionResult> SelectRoom()
        {
            // Nếu có TempData thì dùng, không thì tự gọi lại API lấy danh sách phòng
            List<PRNFE.MVC.Models.Response.RoomResponse> rooms = null;
            if (TempData["TenantRooms"] != null)
            {
                var roomsJson = TempData["TenantRooms"] as string;
                rooms = JsonConvert.DeserializeObject<List<PRNFE.MVC.Models.Response.RoomResponse>>(roomsJson);
            }
            else
            {
                // Lấy access token từ cookie
                var accessToken = Request.Cookies["AccessToken"];
                if (string.IsNullOrEmpty(accessToken))
                {
                    TempData["Message"] = "Vui lòng đăng nhập lại!";
                    return RedirectToAction("Login");
                }
                var apiUrl = $"{_apiBaseUrl}/residents/api/Rooms/user";
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.SendAsync(requestMessage);
                var result = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<PRNFE.MVC.Models.Response.ApiResponse<List<PRNFE.MVC.Models.Response.RoomResponse>>>(result);
                if (apiResponse.success && apiResponse.data != null)
                {
                    rooms = apiResponse.data;
                }
            }
            if (rooms == null || rooms.Count == 0)
            {
                TempData["Message"] = "Không tìm thấy danh sách phòng.";
                return RedirectToAction("Login");
            }
            return View(rooms);
        }

        [HttpPost]
        public IActionResult SelectRoom(int roomId, int buildingId)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Để test local, nếu dùng HTTPS thì để true
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("RoomId", roomId.ToString(), options);
            Response.Cookies.Append("BuildingId", buildingId.ToString(), options);
            return RedirectToAction("InvoiceInfo", "Tenant");
        }

        [HttpPost]
        public new IActionResult Logout()
        {
            // Xóa toàn bộ cookie liên quan
            foreach (var cookieKey in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookieKey);
            }
            base.Logout();
            return RedirectToAction("Login");
        }
    }
}