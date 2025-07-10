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
    public class RoomsController : BaseController
    {
        public RoomsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int size = 10)
        {
            try
            {
                if (!ValidateBuildingId())
                {
                    TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Login", "Account");
                }

                using var httpClient = CreateHttpClientWithCookies();

                var buildingId = GetBuildingId();
                httpClient.DefaultRequestHeaders.Add("buildingId", buildingId);

                var apiUrl = $"api/Rooms?$skip={((page - 1) * size)}&$top={size}";
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var rooms = JsonConvert.DeserializeObject<List<RoomResponse>>(json);

                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = size;

                    ViewBag.Rooms = rooms ?? new List<RoomResponse>();

                    // Get room statistics
                    ViewBag.Statistics = await GetRoomStatistics();

                    return View(rooms);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể tải danh sách phòng: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tải danh sách phòng!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return View(new List<RoomResponse>());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var response = await httpClient.GetAsync($"api/Rooms/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var room = JsonConvert.DeserializeObject<DetailRoomResponse>(json);

                    if (room != null)
                    {
                        // Get additional data for display
                        ViewBag.RoomTypes = await GetRoomTypes();
                        ViewBag.AvailableResidents = await GetAvailableResidents();
                        ViewBag.AvailableServices = await GetAvailableServices();

                        return View(room);
                    }
                }

                TempData["Message"] = "Không tìm thấy phòng!";
                TempData["IsSuccess"] = false;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tải thông tin phòng!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RoomTypes = await GetRoomTypes();
            ViewBag.AvailableResidents = await GetAvailableResidents();
            ViewBag.AvailableServices = await GetAvailableServices();
            ViewBag.BuildingId = GetBuildingId();

            return View(new CreateRoomRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoomTypes = await GetRoomTypes();
                ViewBag.AvailableResidents = await GetAvailableResidents();
                ViewBag.AvailableServices = await GetAvailableServices();
                ViewBag.BuildingId = GetBuildingId();
                return View(request);
            }

            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                ViewBag.RoomTypes = await GetRoomTypes();
                return View(request);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                // Thêm buildingId vào header
                var buildingId = GetBuildingId();
                httpClient.DefaultRequestHeaders.Add("buildingId", buildingId);

                // Tạo một đối tượng mới bao gồm cả buildingId
                var requestWithBuildingId = new
                {
                    buildingId = int.Parse(buildingId),
                    roomNumber = request.RoomNumber,
                    floor = request.Floor,
                    area = request.Area,
                    roomTypeId = request.RoomTypeId,
                    maxOpt = request.MaxOpt,
                    notes = request.Notes,
                    initialResidentIds = request.InitialResidentIds,
                    initialServiceIds = request.InitialServiceIds
                };

                var json = JsonConvert.SerializeObject(requestWithBuildingId);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Creating room with buildingId: {buildingId}");
                Console.WriteLine($"Request data: {json}");

                var response = await httpClient.PostAsync("api/Rooms", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var createdRoom = JsonConvert.DeserializeObject<RoomResponse>(responseJson);

                    TempData["Message"] = "Tạo phòng thành công!";
                    TempData["IsSuccess"] = true;

                    // If there are initial residents or services, redirect to edit
                    if (request.InitialResidentIds.Any() || request.InitialServiceIds.Any())
                    {
                        return RedirectToAction(nameof(Edit), new { id = createdRoom?.Id });
                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể tạo phòng: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tạo phòng!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.RoomTypes = await GetRoomTypes();
            ViewBag.AvailableResidents = await GetAvailableResidents();
            ViewBag.AvailableServices = await GetAvailableServices();
            ViewBag.BuildingId = GetBuildingId();
            return View(request);
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (!ValidateBuildingId())
                {
                    TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction(nameof(Index));
                }

                using var httpClient = CreateHttpClientWithCookies();

                // Thêm buildingId vào header
                var buildingId = GetBuildingId();
                httpClient.DefaultRequestHeaders.Add("buildingId", buildingId);

                var response = await httpClient.GetAsync($"api/Rooms/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var room = JsonConvert.DeserializeObject<DetailRoomResponse>(json);

                    if (room != null)
                    {
                        var updateRequest = new UpdateRoomRequest
                        {
                            TenantId = room.TenantId,
                            RoomNumber = room.RoomNumber,
                            Floor = room.Floor,
                            Area = room.Area,
                            RoomTypeId = room.RoomTypeId,
                            MaxOpt = room.MaxOpt,
                            Status = room.Status,
                            Notes = string.Empty, // Set default value since Notes might not exist in DetailRoomResponse
                            Residents = room.Residents?.Select(r => new UpdateResidentInRoomRequest
                            {
                                ResidentId = r.ResidentId,
                                IsActive = r.IsActive,
                                JoinDate = r.JoinDate,
                                LeaveDate = r.LeaveDate,
                                Notes = r.Notes ?? string.Empty,
                                Resident = r.Resident
                            }).ToList() ?? new List<UpdateResidentInRoomRequest>(),
                            Services = room.Services?.Select(s => new UpdateServiceInRoomRequest
                            {
                                ServiceId = s.ServiceId,
                                CustomPrice = s.CustomPrice,
                                IsActive = s.IsActive,
                                StartDate = s.StartDate,
                                EndDate = s.EndDate,
                                Notes = s.Notes ?? string.Empty,
                                Service = s.Service
                            }).ToList() ?? new List<UpdateServiceInRoomRequest>()
                        };

                        ViewBag.RoomId = id;
                        ViewBag.RoomTypes = await GetRoomTypes();
                        ViewBag.AvailableResidents = await GetAvailableResidents();
                        ViewBag.AvailableServices = await GetAvailableServices();
                        ViewBag.RoomHistory = room.History ?? new List<RoomHistoryResponse>();
                        ViewBag.BuildingId = buildingId;

                        return View(updateRequest);
                    }
                }

                TempData["Message"] = "Không tìm thấy phòng!";
                TempData["IsSuccess"] = false;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tải thông tin phòng!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateRoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoomId = id;
                ViewBag.RoomTypes = await GetRoomTypes();
                ViewBag.AvailableResidents = await GetAvailableResidents();
                ViewBag.AvailableServices = await GetAvailableServices();
                ViewBag.BuildingId = GetBuildingId();
                return View(request);
            }

            if (!ValidateBuildingId())
            {
                TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                TempData["IsSuccess"] = false;
                ViewBag.RoomId = id;
                ViewBag.RoomTypes = await GetRoomTypes();
                ViewBag.AvailableResidents = await GetAvailableResidents();
                ViewBag.AvailableServices = await GetAvailableServices();
                ViewBag.BuildingId = GetBuildingId();
                return View(request);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                // Thêm buildingId vào header
                var buildingId = GetBuildingId();
                httpClient.DefaultRequestHeaders.Add("buildingId", buildingId);

                // Tạo đối tượng request với buildingId
                var requestWithBuildingId = new
                {
                    buildingId = int.Parse(buildingId),
                    tenantId = request.TenantId,
                    roomNumber = request.RoomNumber,
                    floor = request.Floor,
                    area = request.Area,
                    roomTypeId = request.RoomTypeId,
                    maxOpt = request.MaxOpt,
                    status = request.Status,
                    notes = request.Notes ?? string.Empty,
                    residents = request.Residents?.Select(r => new
                    {
                        residentId = r.ResidentId,
                        isActive = r.IsActive,
                        joinDate = r.JoinDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        leaveDate = r.LeaveDate?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        notes = r.Notes ?? string.Empty
                    }).ToList(),
                    services = request.Services?.Select(s => new
                    {
                        serviceId = s.ServiceId,
                        customPrice = s.CustomPrice,
                        isActive = s.IsActive,
                        startDate = s.StartDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        endDate = s.EndDate?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        notes = s.Notes ?? string.Empty
                    }).ToList()
                };

                var json = JsonConvert.SerializeObject(requestWithBuildingId, new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    NullValueHandling = NullValueHandling.Ignore
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Updating room {id} with buildingId: {buildingId}");
                Console.WriteLine($"Request data: {json}");

                var response = await httpClient.PutAsync($"api/Rooms/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Cập nhật phòng thành công!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể cập nhật phòng: {errorContent}";
                    TempData["IsSuccess"] = false;
                    Console.WriteLine($"Error response: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi cập nhật phòng!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.RoomId = id;
            ViewBag.RoomTypes = await GetRoomTypes();
            ViewBag.AvailableResidents = await GetAvailableResidents();
            ViewBag.AvailableServices = await GetAvailableServices();
            ViewBag.BuildingId = GetBuildingId();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var response = await httpClient.DeleteAsync($"api/Rooms/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Xóa phòng thành công!";
                    TempData["IsSuccess"] = true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể xóa phòng: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi xóa phòng!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Filter()
        {
            ViewBag.RoomTypes = await GetRoomTypes();
            return View(new FilterRoomRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Filter(FilterRoomRequest request)
        {
            try
            {
                if (!ValidateBuildingId())
                {
                    TempData["Message"] = "Không tìm thấy thông tin tòa nhà. Vui lòng đăng nhập lại!";
                    TempData["IsSuccess"] = false;
                    ViewBag.RoomTypes = await GetRoomTypes();
                    return View(request);
                }

                var filterDto = new
                {
                    roomNumber = request.RoomNumber ?? string.Empty,
                    floor = request.Floor,
                    roomTypeId = request.RoomTypeId,
                    status = request.Status,
                    minArea = request.MinArea,
                    maxArea = request.MaxArea,
                    minMaxOpt = request.MinMaxOpt,
                    maxMaxOpt = request.MaxMaxOpt,
                    hasResidents = request.HasResidents,
                    hasServices = request.HasServices,
                    createdFrom = request.CreatedFrom?.ToString("yyyy-MM-dd"),
                    createdTo = request.CreatedTo?.ToString("yyyy-MM-dd"),
                    sortBy = request.SortBy,
                    sortOrder = request.SortOrder
                };

                using var httpClient = CreateHttpClientWithCookies();

                var json = JsonConvert.SerializeObject(filterDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending filter request: {json}");

                var response = await httpClient.PostAsync("api/Rooms/filter", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var rooms = JsonConvert.DeserializeObject<List<RoomResponse>>(responseJson);

                    // Apply PageSize limit
                    if (rooms != null && rooms.Count > request.PageSize)
                    {
                        rooms = rooms.Take(request.PageSize).ToList();
                    }

                    ViewBag.FilteredRooms = rooms ?? new List<RoomResponse>();
                    ViewBag.FilterApplied = true;
                    TempData["Message"] = $"Tìm thấy {rooms?.Count ?? 0} phòng phù hợp!";
                    TempData["IsSuccess"] = true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.FilteredRooms = new List<RoomResponse>();
                    ViewBag.FilterApplied = true;
                    TempData["Message"] = $"Không thể lọc phòng: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                ViewBag.FilteredRooms = new List<RoomResponse>();
                ViewBag.FilterApplied = true;
                TempData["Message"] = "Có lỗi xảy ra khi lọc phòng!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.RoomTypes = await GetRoomTypes();
            return View(request);
        }

        // Bulk Operations
        [HttpGet]
        public async Task<IActionResult> BulkUpdate()
        {
            ViewBag.RoomTypes = await GetRoomTypes();
            ViewBag.AllRooms = await GetAllRooms();
            return View(new BulkUpdateRoomRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdate(BulkUpdateRoomRequest request)
        {
            if (!ModelState.IsValid || !request.RoomIds.Any())
            {
                ViewBag.RoomTypes = await GetRoomTypes();
                ViewBag.AllRooms = await GetAllRooms();
                TempData["Message"] = "Vui lòng chọn ít nhất một phòng và điền đầy đủ thông tin!";
                TempData["IsSuccess"] = false;
                return View(request);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/Rooms/bulk-update", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"Cập nhật thành công {request.RoomIds.Count} phòng!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể cập nhật: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi cập nhật hàng loạt!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.RoomTypes = await GetRoomTypes();
            ViewBag.AllRooms = await GetAllRooms();
            return View(request);
        }

        // Room Transfer
        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            ViewBag.AllRooms = await GetAllRooms();
            return View(new RoomTransferRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(RoomTransferRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllRooms = await GetAllRooms();
                return View(request);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/Rooms/transfer", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Chuyển phòng thành công!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Details), new { id = request.ToRoomId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể chuyển phòng: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi chuyển phòng!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.AllRooms = await GetAllRooms();
            return View(request);
        }

        // Maintenance
        [HttpGet]
        public async Task<IActionResult> Maintenance(int? roomId = null)
        {
            ViewBag.AllRooms = await GetAllRooms();
            var request = new RoomMaintenanceRequest();
            if (roomId.HasValue)
            {
                request.RoomId = roomId.Value;
            }
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Maintenance(RoomMaintenanceRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllRooms = await GetAllRooms();
                return View(request);
            }

            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/Rooms/maintenance", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Tạo yêu cầu bảo trì thành công!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction(nameof(Details), new { id = request.RoomId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"Không thể tạo yêu cầu bảo trì: {errorContent}";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi xảy ra khi tạo yêu cầu bảo trì!";
                TempData["IsSuccess"] = false;
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.AllRooms = await GetAllRooms();
            return View(request);
        }

        // API Endpoints for AJAX
        [HttpGet]
        public async Task<IActionResult> GetRoomDetails(int roomId)
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var response = await httpClient.GetAsync($"api/Rooms/{roomId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var room = JsonConvert.DeserializeObject<DetailRoomResponse>(json);

                    return Json(new { success = true, room = room });
                }

                return Json(new { success = false, message = "Không tìm thấy phòng" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoomStatus(int roomId, int status)
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var request = new { status = status };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PatchAsync($"api/Rooms/{roomId}/status", content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false });
            }
        }

        // Helper methods
        private async Task<List<RoomTypeOption>> GetRoomTypes()
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var response = await httpClient.GetAsync("api/RoomTypes");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var roomTypes = JsonConvert.DeserializeObject<List<RoomTypeResponse>>(json);

                    return roomTypes?.Select(rt => new RoomTypeOption
                    {
                        Id = rt.Id,
                        Name = rt.Name,
                        Description = rt.Description
                    }).ToList() ?? new List<RoomTypeOption>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting room types: {ex.Message}");
            }

            return new List<RoomTypeOption>();
        }

        private async Task<List<ResidentOption>> GetAvailableResidents()
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var response = await httpClient.GetAsync("api/Residents");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var residents = JsonConvert.DeserializeObject<List<ResidentResponse>>(json);

                    return residents?.Where(r => r.IsActive).Select(r => new ResidentOption
                    {
                        Id = r.Id,
                        FullName = r.FullName,
                        Email = r.Email,
                        Phone = r.Phone,
                        IsActive = r.IsActive
                    }).ToList() ?? new List<ResidentOption>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting residents: {ex.Message}");
            }

            return new List<ResidentOption>();
        }

        private async Task<List<ServiceOption>> GetAvailableServices()
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var response = await httpClient.GetAsync("api/Services");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var services = JsonConvert.DeserializeObject<List<ServiceResponse>>(json);

                    return services?.Where(s => s.IsActive).Select(s => new ServiceOption
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Unit = s.Unit,
                        PricePerUnit = s.PricePerUnit,
                        IsMandatory = s.IsMandatory
                    }).ToList() ?? new List<ServiceOption>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting services: {ex.Message}");
            }

            return new List<ServiceOption>();
        }

        private async Task<List<RoomResponse>> GetAllRooms()
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();

                var buildingId = GetBuildingId();
                httpClient.DefaultRequestHeaders.Add("buildingId", buildingId);

                var response = await httpClient.GetAsync("api/Rooms");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<RoomResponse>>(json) ?? new List<RoomResponse>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all rooms: {ex.Message}");
            }

            return new List<RoomResponse>();
        }

        private async Task<object> GetRoomStatistics()
        {
            try
            {
                using var httpClient = CreateHttpClientWithCookies();
                var response = await httpClient.GetAsync("api/Rooms/statistics");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting room statistics: {ex.Message}");
            }

            return new { };
        }

        // Helper method to get room status text
        public static string GetRoomStatusText(int status) => status switch
        {
            0 => "Trống",
            1 => "Đã thuê",
            2 => "Đã đặt",
            3 => "Bảo trì",
            4 => "Không sử dụng",
            5 => "Chờ dọn dẹp",
            6 => "Sắp hết hạn",
            7 => "Tạm khóa",
            _ => "Không xác định"
        };

        public static string GetRoomStatusColor(int status) => status switch
        {
            0 => "success",
            1 => "primary",
            2 => "info",
            3 => "warning",
            4 => "danger",
            5 => "secondary",
            6 => "warning",
            7 => "dark",
            _ => "secondary"
        };
    }

    // Helper classes
    public class RoomTypeOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ResidentOption
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ServiceOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal PricePerUnit { get; set; }
        public bool IsMandatory { get; set; }
    }
}