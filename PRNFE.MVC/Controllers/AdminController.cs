using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PRNFE.MVC.Models.Response;
using PRNFE.MVC.Attributes;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;

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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Permission");
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Role");
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Role/{id}");
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
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Role", content);
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
                
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/api/Role/{id}", content);
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
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/Role/{id}");
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
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Role/assign-permissions", content);
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
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Role/assign-users", content);
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Role/{id}/users");
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
    }
} 