using Microsoft.AspNetCore.Mvc;
using PRNFE.MVC.Models.Request;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[Route("support-requests")]
public class SupportRequestsController : Controller
{
    private readonly HttpClient _httpClient;

    public SupportRequestsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    private string GetBuildingIdFromCookie()
    {
        var buildingId = Request.Cookies["buildingId"];
        if (string.IsNullOrEmpty(buildingId))
        {
            throw new InvalidOperationException("buildingId cookie không tồn tại hoặc rỗng.");
        }
        return buildingId;
    }

    private void AddBuildingIdCookie()
    {
        var buildingId = GetBuildingIdFromCookie();
        _httpClient.DefaultRequestHeaders.Add("Cookie", $"buildingId={buildingId}");
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        AddBuildingIdCookie();
        var response = await _httpClient.GetAsync("/api/SupportRequests");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(content);
        var data = json.RootElement.GetProperty("data").Deserialize<List<SupportRequests>>();

        return View(data);
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        AddBuildingIdCookie();
        var response = await _httpClient.GetAsync($"/api/SupportRequests/{id}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(content);
        var data = json.RootElement.GetProperty("data").Deserialize<SupportRequestDetailModel>();

        return View(data);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] SupportRequests model)
    {
        AddBuildingIdCookie();

        var postData = new
        {
            residentId = 0, // bạn cần xử lý đúng residentId
            requestType = model.RequestType,
            description = model.Description,
            imgUrl = model.ImgUrl
        };

        var content = new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/SupportRequests", content);
        response.EnsureSuccessStatusCode();

        return RedirectToAction("Index");
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(int id, [FromForm] SupportRequestDetailModel model)
    {
        AddBuildingIdCookie();

        var updateData = new
        {
            requestType = model.RequestType,
            description = model.Description,
            imgUrl = model.ImgUrl,
            handleNote = model.HandleNote,
            status = model.Status
        };

        var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/api/SupportRequests/{id}", content);
        response.EnsureSuccessStatusCode();

        return RedirectToAction("Index");
    }

    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        AddBuildingIdCookie();
        var response = await _httpClient.DeleteAsync($"/api/SupportRequests/{id}");
        response.EnsureSuccessStatusCode();
        return RedirectToAction("Index");
    }

    [HttpGet("filter")]
    public async Task<IActionResult> Filter(int? requestType, int? status, int[] residentIds, int[] roomIds)
    {
        AddBuildingIdCookie();

        var query = new List<string>();
        if (requestType.HasValue)
            query.Add($"RequestType={requestType.Value}");
        if (status.HasValue)
            query.Add($"Status={status.Value}");
        foreach (var r in residentIds)
            query.Add($"ResidentIds={r}");
        foreach (var r in roomIds)
            query.Add($"RoomIds={r}");

        string queryString = string.Join("&", query);
        var response = await _httpClient.GetAsync($"/api/SupportRequests/filter?{queryString}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var data = json.RootElement.GetProperty("data").Deserialize<List<SupportRequests>>();

        return View("Index", data);
    }
}
