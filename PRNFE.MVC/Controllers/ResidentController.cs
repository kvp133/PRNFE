
using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace PRNFE.MVC.Controllers
{
    public class ResidentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly string _apiBaseUrl;

        public ResidentController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration.GetSection("ApiSettings:BaseUrl").Value ?? throw new InvalidOperationException("API BaseUrl is not configured.");
        }

        // INDEX
        public async Task<IActionResult> Index(ResidentFilterRequest filter)
        {
            filter.BuildingId = filter.BuildingId == 0 ? 1 : filter.BuildingId;
            var queryParams = new List<string>
            {
                $"BuildingId={filter.BuildingId}",
                $"Page={filter.Page}",
                $"PageSize={filter.PageSize}"
            };

            if (!string.IsNullOrEmpty(filter.FullName)) queryParams.Add($"FullName={Uri.EscapeDataString(filter.FullName)}");
            if (!string.IsNullOrEmpty(filter.PhoneNumber)) queryParams.Add($"PhoneNumber={Uri.EscapeDataString(filter.PhoneNumber)}");
            if (!string.IsNullOrEmpty(filter.RoomNumber)) queryParams.Add($"RoomNumber={Uri.EscapeDataString(filter.RoomNumber)}");

            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Residents/filters?{string.Join("&", queryParams)}");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"API Error: {response.StatusCode}";
                    return View(new ResidentFilterResponse());
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResidentFilterResponse>(json, _jsonOptions);
                ViewBag.Filter = filter;
                return View(result ?? new ResidentFilterResponse());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading residents: {ex.Message}";
                ViewBag.Filter = filter;
                return View(new ResidentFilterResponse());
            }
        }

        // CREATE - GET
        public IActionResult Create() => View(new ResidentRequest
        {
            DateOfBirth = DateTime.Now.AddYears(-18),
            Vehicles = new List<VehicleCreateDto>(),
            Rooms = new List<RoomCreateDto>()
        });

        // CREATE - POST
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResidentRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Residents",
                    new StringContent(JsonSerializer.Serialize(model, _jsonOptions), System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Resident added successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.IsSuccess = false;
                ViewBag.Message = $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = $"Error: {ex.Message}";
            }
            return View(model);
        }

        // EDIT - GET
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Residents/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Resident with ID {id} not found";
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                var resident = JsonSerializer.Deserialize<ResidentResponse>(json, _jsonOptions);
                if (resident == null)
                {
                    TempData["ErrorMessage"] = $"Resident with ID {id} not found";
                    return RedirectToAction(nameof(Index));
                }

                var model = new ResidentUpdateRequest
                {
                    UserId = resident.UserId,
                    FullName = resident.FullName,
                    PhoneNumber = resident.PhoneNumber,
                    Email = resident.Email,
                    DateOfBirth = resident.DateOfBirth,
                    Address = resident.Address,
                    Gender = resident.Gender,
                    Rooms = resident.Rooms?.Select(r => new RoomCreateDto { RoomId = r.RoomId }).ToList() ?? new List<RoomCreateDto>(),
                    Vehicles = resident.Vehicles?.Select(v => new VehicleUpdateDto
                    {
                        Type = v.Type,
                        LicensePlate = v.LicensePlate
                    }).ToList() ?? new List<VehicleUpdateDto>(),
                    TemporaryStay = resident.TemporaryStay != null ? new TemporaryStayUpdateDto
                    {
                        FromDate = resident.TemporaryStay.FromDate,
                        ToDate = resident.TemporaryStay.ToDate,
                        Note = resident.TemporaryStay.Note,
                        Status = resident.TemporaryStay.Status
                    } : null
                };

                ViewBag.ResidentId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading resident: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT - POST
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResidentUpdateRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ResidentId = id;
                return View(model);
            }

            try
            {
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/api/Residents/{id}",
                    new StringContent(JsonSerializer.Serialize(model, _jsonOptions), System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Resident updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.IsSuccess = false;
                ViewBag.Message = $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
            }
            catch (Exception ex)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = $"Error: {ex.Message}";
            }

            ViewBag.ResidentId = id;
            return View(model);
        }

        // DELETE
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/Residents/{id}");
                TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                    response.IsSuccessStatusCode ? "Resident deleted successfully!" : $"Error deleting resident: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Residents/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Resident not found";
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                var resident = JsonSerializer.Deserialize<ResidentResponse>(json, _jsonOptions);
                if (resident == null)
                {
                    TempData["ErrorMessage"] = "Resident not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(resident);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
