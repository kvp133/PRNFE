using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PRNFE.MVC.Models.Request;
using PRNFE.MVC.Models.Response;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace PRNFE.MVC.Controllers
{
    public class TemporaryStaysController : BaseController
    {
        public TemporaryStaysController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base(httpClientFactory, configuration)
        {
        }

        // GET: TemporaryStaysController
        public async Task<ActionResult> Index()
        {
            var apiUrl = $"{_apiQLPTUrl}/api/TemporaryStays";          
            await SetFilterData();
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<TemporaryStayResponse>>>(apiUrl);
            

            return View(response.data);
        }

        public async Task<ActionResult> Filter(FilterTemporaryStayDto model)
        {
            string message = string.Empty;
            try
            {
                ViewBag.FilterModel = model;
                await SetFilterData();
                var apiUrl = $"{_apiQLPTUrl}/api/TemporaryStays/filters";
                var queryParams = new List<string>();
                if (model.ResidentIds != null && model.ResidentIds.Count > 0)
                {
                    queryParams.Add($"residentIds={string.Join(",", model.ResidentIds)}");
                }
                if (model.RoomIds != null && model.RoomIds.Count > 0)
                {
                    queryParams.Add($"roomIds={string.Join(",", model.RoomIds)}");
                }
                if (model.Status.HasValue)
                {
                    queryParams.Add($"status={model.Status.Value}");
                }
                if (queryParams.Count > 0)
                {
                    apiUrl += "?" + string.Join("&", queryParams);
                }
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<TemporaryStayResponse>>>(apiUrl);
                message = response.message;
                return View("Index",response.data);
            }
            catch
            {
                Console.WriteLine("Fail to run login in Filter(FilterTemporaryStayDto model)");
                Console.WriteLine(message);
                return View("Index");
            }
        }

        // GET: TemporaryStaysController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var apiUrl = $"{_apiQLPTUrl}/api/TemporaryStays/{id}";

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<DetailTemporaryStayResponse>>(apiUrl);
            return View(response.data);
        }


        // GET: TemporaryStaysController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var apiUrl = $"{_apiQLPTUrl}/api/TemporaryStays/{id}";

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<UpdateTemporaryStayDto>>(apiUrl);
            return View(response.data);
        }

        // POST: TemporaryStaysController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UpdateTemporaryStayDto collection)
        {
            try
            {
                var apiUrl = $"{_apiQLPTUrl}/api/TemporaryStays/{id}";
                var content = new StringContent(JsonConvert.SerializeObject(collection)
                    , Encoding.UTF8, "application/json");
                var response = _httpClient.PutAsync(apiUrl, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Optionally, you can redirect to Index or another view after successful edit
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Handle error response
                    var errorMessage = response.Content.ReadAsStringAsync().Result;
                    ModelState.AddModelError(string.Empty, $"Error updating temporary stay: {errorMessage}");
                }
                return View();
            }
            catch
            {
                return View();
            }
        }

        // GET: TemporaryStaysController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        // POST: TemporaryStaysController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            var apiUrl = $"{_apiQLPTUrl}/api/TemporaryStays/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                // Optionally, you can redirect to Index or another view after deletion
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Handle error response
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error deleting temporary stay: {errorMessage}");
            }
            return View();
        }

        private async Task SetFilterData()
        {
            // get room
            var roomApiUrl = $"{_apiQLPTUrl}/api/Rooms";
            var roomResponse = await _httpClient.GetFromJsonAsync<ApiResponse<List<RoomResponse>>>(roomApiUrl);
            // get resident
            var residentApiUrl = $"{_apiQLPTUrl}/api/Residents";
            var residentResponse = await _httpClient.GetFromJsonAsync<ApiResponse<List<ResidentResponse>>>(residentApiUrl);
            // Prepare the view model
            ViewBag.Rooms = roomResponse.data.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RoomNumber
            }).ToList();
            ViewBag.Residents = residentResponse.data.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.FullName
            }).ToList();
        }
    }
}
