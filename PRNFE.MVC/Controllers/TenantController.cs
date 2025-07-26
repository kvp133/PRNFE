using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRNFE.MVC.Attributes;
using PRNFE.MVC.Helper;
using PRNFE.MVC.Models.Response.dat;
using System.Text;

namespace PRNFE.MVC.Controllers
{
   
    public class TenantController : BaseController
    {
		private readonly HttpClient _client;

		public TenantController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
			: base(httpClientFactory, configuration)
		{
			_client = httpClientFactory.CreateClient();
		}



		public IActionResult InvoiceInfo()
		{
			var userInfo = GetUserInfo();
			ViewBag.UserInfo = userInfo;

			// Tạo dữ liệu giả cho phương tiện
			var mockVehicles = new List<VehicleResponsesDat>
	        {
		        new VehicleResponsesDat
		        {
			        Id = "1",
			        Type = "0", // 0 = Car, hoặc bạn có enum cũng được
                    LicensePlate = "59A-123.45",
			        CreateAt = DateTime.Now.AddMonths(-2),
			        UpdatedAt = DateTime.Now
		        },
		        new VehicleResponsesDat
				{
			        Id = "1",
			        Type = "1", // 1 = Motorbike
                    LicensePlate = "84B-456.78",
			        CreateAt = DateTime.Now.AddMonths(-1),
			        UpdatedAt = DateTime.Now
		        }
	        };

			return View(mockVehicles);
		}


		public IActionResult ManageVehicle()
		{
			var userInfo = GetUserInfo();
			ViewBag.UserInfo = userInfo;

			TempData["ToastMessage"] = "Cập nhật thành công!";
			TempData["ToastType"] = "success"; // hoặc "error", "warning"


			// Dữ liệu mock
			var vehicles = new List<VehicleResponsesDat>
			{
				new VehicleResponsesDat { Id = "1", Type = "2", LicensePlate = "59A-888.88", CreateAt = DateTime.Now.AddMonths(-2) },
				new VehicleResponsesDat { Id = "2", Type = "0", LicensePlate = "84B-123.45", CreateAt = DateTime.Now.AddMonths(-1) }
			};

			return View(vehicles);
		}



		public async Task<IActionResult> ManagePayment(int page = 1, int size = 5, string orderCode = null)
		{
			var buildingId = Request.Cookies["buildingId"];
			var roomId = Request.Cookies["roomId"];

			if (string.IsNullOrEmpty(buildingId) || string.IsNullOrEmpty(roomId))
			{
				TempData["ToastMessage"] = "Không tìm thấy thông tin phòng.";
				TempData["ToastType"] = "error";
				return RedirectToAction("SelectRoom", "Room");
			}

			// ✅ Nếu có orderCode → gọi PayOS để check trạng thái
			if (!string.IsNullOrEmpty(orderCode))
			{
				try
				{
					var payBaseUrl = GetBaseUrl.UrlPayment?.TrimEnd('/');
					var statusUrl = $"{payBaseUrl}/api/Payment/payos/status/{orderCode}";

					var statusResp = await _httpClient.GetAsync(statusUrl);
					if (statusResp.IsSuccessStatusCode)
					{
						var statusJson = await statusResp.Content.ReadAsStringAsync();
						var statusResult = JsonConvert.DeserializeObject<PayOSCreatePaymentResponse>(statusJson);

						string status = statusResult?.Data?.Status?.ToUpperInvariant();

						string message = status switch
						{
							"PAID" => $"💰 Thanh toán thành công cho đơn hàng #{orderCode}!",
							"PROCESSING" => "🔄 Đang xử lý thanh toán...",
							"PENDING" => "⏳ Chưa hoàn tất thanh toán.",
							"CANCELLED" => "❌ Giao dịch đã bị hủy.",
							_ => "⚠️ Trạng thái thanh toán không xác định."
						};

						string type = status == "PAID" ? "success"
									: status == "CANCELLED" ? "warning"
									: "info";

						TempData["ToastMessage"] = message;
						TempData["ToastType"] = type;
					}
					else
					{
						TempData["ToastMessage"] = "Không kiểm tra được trạng thái thanh toán từ PayOS.";
						TempData["ToastType"] = "warning";
					}
				}
				catch (Exception ex)
				{
					TempData["ToastMessage"] = "Lỗi khi kiểm tra trạng thái thanh toán: " + ex.Message;
					TempData["ToastType"] = "error";
				}
			}

			// 🔁 Load danh sách hóa đơn
			var baseUrl = GetBaseUrl.UrlPayment?.TrimEnd('/');
			var url = $"{baseUrl}/api/Invoice/list-by-room-building" +
					  $"?buildingId={buildingId}&roomId={roomId}&page={page}&pageSize={size}";

			try
			{
				var response = await _httpClient.GetAsync(url);
				if (!response.IsSuccessStatusCode)
				{
					TempData["ToastMessage"] = $"Lỗi khi gọi API hóa đơn: {response.StatusCode}";
					TempData["ToastType"] = "error";
					return View(new PaginatedData<InvoiceResponseDat>());
				}

				var json = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<ApiResult<PaginatedData<InvoiceResponseDat>>>(json);

				if (result == null || !result.Success || result.Data == null)
				{
					TempData["ToastMessage"] = result?.Message ?? "Lỗi dữ liệu từ hệ thống.";
					TempData["ToastType"] = "error";
					return View(new PaginatedData<InvoiceResponseDat>());
				}

				return View(result.Data);
			}
			catch (Exception ex)
			{
				TempData["ToastMessage"] = "Đã xảy ra lỗi khi xử lý dữ liệu.";
				TempData["ToastType"] = "error";
				return View(new PaginatedData<InvoiceResponseDat>());
			}
		}





		public async Task<IActionResult> GetBillDetail(string billId, string invoiceId)
		{
			try
			{
				if (string.IsNullOrEmpty(billId))
				{
					TempData["ToastMessage"] = "Thiếu mã hóa đơn.";
					TempData["ToastType"] = "error";
					return RedirectToAction("ManagePayment");
				}

				var baseUrl = GetBaseUrl.UrlQlpt?.TrimEnd('/');
				var url = $"{baseUrl}/api/Bills/{billId}";

				var response = await _httpClient.GetAsync(url);
				if (!response.IsSuccessStatusCode)
				{
					TempData["ToastMessage"] = "Không thể tải dữ liệu hóa đơn chi tiết.";
					TempData["ToastType"] = "error";
					return RedirectToAction("ManagePayment");
				}

				var json = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<ApiResult<BillResponseDat>>(json);

				if (result == null || !result.Success || result.Data == null)
				{
					TempData["ToastMessage"] = result?.Message ?? "Lỗi khi tải chi tiết hóa đơn.";
					TempData["ToastType"] = "error";
					return RedirectToAction("ManagePayment");
				}

				ViewBag.InvoiceId = invoiceId;
				return View(result.Data);
			}
			catch (Exception ex)
			{
				TempData["ToastMessage"] = "Đã xảy ra lỗi khi xử lý dữ liệu: " + ex.Message;
				TempData["ToastType"] = "error";
				return RedirectToAction("ManagePayment");
			}
		}



		[HttpPost]
		public async Task<IActionResult> PayInvoice(string invoiceId, decimal amount)
		{
			if (string.IsNullOrEmpty(invoiceId) || !int.TryParse(invoiceId, out var orderCode))
			{
				TempData["ToastMessage"] = "Thông tin thanh toán không hợp lệ.";
				TempData["ToastType"] = "error";
				return RedirectToAction("ManagePayment");
			}

			try
			{
				var payload = new
				{
					
						orderCode = 11111111,
						amount = (int)2000,
						description = $"Thanh toán hóa đơn {orderCode}"
					
				};

				var baseUrl = GetBaseUrl.UrlPayment?.TrimEnd('/');
				var url = $"{baseUrl}/api/Payment/payos/create";

				var jsonPayload = JsonConvert.SerializeObject(payload);
				var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(url, content);
				if (!response.IsSuccessStatusCode)
				{
					var err = await response.Content.ReadAsStringAsync();
					TempData["ToastMessage"] = $"Không thể tạo thanh toán: {err}";
					TempData["ToastType"] = "error";
					return RedirectToAction("ManagePayment");
				}

				var json = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<PayOSCreatePaymentResponse>(json);

				if (result?.Data?.CheckoutUrl != null)
				{
					return Redirect(result.Data.CheckoutUrl); // ✅ redirect đến PayOS
				}

				TempData["ToastMessage"] = "Không nhận được URL thanh toán từ PayOS.";
				TempData["ToastType"] = "error";
				return RedirectToAction("ManagePayment");
			}
			catch (Exception ex)
			{
				TempData["ToastMessage"] = "Lỗi khi gọi PayOS: " + ex.Message;
				TempData["ToastType"] = "error";
				return RedirectToAction("ManagePayment");
			}
		}






	}
} 