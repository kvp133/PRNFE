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
        // GET: Admin/UserManagement
        public IActionResult UserManagement()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View("adminpanel");
        }

        // GET: Admin/PermissionManagement
        public IActionResult PermissionManagement()
        {
            var userInfo = GetUserInfo();
            ViewBag.UserInfo = userInfo;
            return View("adminpanel");
        }
        // GET: Admin/GetPermissions
        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/users/api/Permission");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(JsonConvert.DeserializeObject<ApiResponse<List<PermissionResponse>>>(content));
                }
                
                return Json(new ApiResponse<List<PermissionResponse>>
                {
                    success = false,
                    message = "Không thể tải danh sách permissions",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<PermissionResponse>>
                {
                    success = false,
                    message = "Lỗi khi tải permissions",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: Admin/GetRoles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/users/api/Role");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(JsonConvert.DeserializeObject<ApiResponse<List<RoleResponse>>>(content));
                }
                
                return Json(new ApiResponse<List<RoleResponse>>
                {
                    success = false,
                    message = "Không thể tải danh sách roles",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<RoleResponse>>
                {
                    success = false,
                    message = "Lỗi khi tải roles",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: Admin/GetRole/{id}
        [HttpGet]
        public async Task<IActionResult> GetRole(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/users/api/Role/{id}");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(JsonConvert.DeserializeObject<ApiResponse<RoleResponse>>(content));
                }
                
                return Json(new ApiResponse<RoleResponse>
                {
                    success = false,
                    message = "Không thể tải thông tin role",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<RoleResponse>
                {
                    success = false,
                    message = "Lỗi khi tải thông tin role",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        // POST: Admin/CreateRole
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/Role", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent));
                }
                
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Không thể tạo role",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Lỗi khi tạo role",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        // PUT: Admin/UpdateRole/{id}
        [HttpPut]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/users/api/Role/{id}", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent));
                }
                
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Không thể cập nhật role",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Lỗi khi cập nhật role",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        // DELETE: Admin/DeleteRole/{id}
        [HttpDelete]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/users/api/Role/{id}");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(new ApiResponse<object>
                    {
                        success = true,
                        message = "Role đã được xóa thành công",
                        data = null,
                        errors = null
                    });
                }
                
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Không thể xóa role",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Lỗi khi xóa role",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        // POST: Admin/AssignPermissions
        [HttpPost]
        public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/Role/assign-permissions", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(new ApiResponse<object>
                    {
                        success = true,
                        message = "Permissions đã được cập nhật thành công",
                        data = null,
                        errors = null
                    });
                }
                
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Không thể cập nhật permissions",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Lỗi khi cập nhật permissions",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        // POST: Admin/AssignUsers
        [HttpPost]
        public async Task<IActionResult> AssignUsers([FromBody] AssignUsersRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/users/api/Role/assign-users", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(new ApiResponse<object>
                    {
                        success = true,
                        message = "User assignments đã được cập nhật thành công",
                        data = null,
                        errors = null
                    });
                }
                
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Không thể cập nhật user assignments",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<object>
                {
                    success = false,
                    message = "Lỗi khi cập nhật user assignments",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: Admin/GetRoleUsers/{id}
        [HttpGet]
        public async Task<IActionResult> GetRoleUsers(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/users/api/Role/{id}/users");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(JsonConvert.DeserializeObject<ApiResponse<List<UserResponse>>>(content));
                }
                
                return Json(new ApiResponse<List<UserResponse>>
                {
                    success = false,
                    message = "Không thể tải danh sách users",
                    data = null,
                    errors = new List<string> { "API Error" }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<UserResponse>>
                {
                    success = false,
                    message = "Lỗi khi tải danh sách users",
                    data = null,
                    errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
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

        [HttpGet]
        public IActionResult adminpanel()
        {
            return View();
        }
    }
} 