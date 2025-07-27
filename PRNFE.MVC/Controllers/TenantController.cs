using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PRNFE.MVC.Attributes;
using PRNFE.MVC.Helper;
using PRNFE.MVC.Models.Response;
using PRNFE.MVC.Models.Response.dat;
using System.Net.Http.Headers;
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



		//public IActionResult InvoiceInfo()
		//{
		//	var userInfo = GetUserInfo();
		//	ViewBag.UserInfo = userInfo;

		//	// Tạo dữ liệu giả cho phương tiện
		//	var mockVehicles = new List<VehicleResponsesDat>
		//       {
		//        new VehicleResponsesDat
		//        {
		//	        Id = "1",
		//	        Type = "0", // 0 = Car, hoặc bạn có enum cũng được
		//                  LicensePlate = "59A-123.45",
		//	        CreateAt = DateTime.Now.AddMonths(-2),
		//	        UpdatedAt = DateTime.Now
		//        },
		//        new VehicleResponsesDat
		//		{
		//	        Id = "1",
		//	        Type = "1", // 1 = Motorbike
		//                  LicensePlate = "84B-456.78",
		//	        CreateAt = DateTime.Now.AddMonths(-1),
		//	        UpdatedAt = DateTime.Now
		//        }
		//       };

		//	return View(mockVehicles);
		//}


		public async Task<IActionResult> ManageVehicle()
		{
			

			var baseUrl = GetBaseUrl.UrlQlpt?.TrimEnd('/');

			// 🔹 Lấy Resident hiện tại
			var userUrl = $"{baseUrl}/api/Residents/user";
			var userResp = await _httpClient.GetAsync(userUrl);

			if (!userResp.IsSuccessStatusCode)
			{
				TempData["ToastMessage"] = "Không lấy được thông tin người dùng.";
				TempData["ToastType"] = "error";
				return View(new List<VehicleResponsesDat>());
			}

			var userJson = await userResp.Content.ReadAsStringAsync();
			var userResult = JsonConvert.DeserializeObject<ApiResult<List<ResidentResponseDat>>>(userJson);

			if (userResult == null || userResult.Data == null || userResult.Data.Count == 0)
			{
				TempData["ToastMessage"] = "Không tìm thấy resident.";
				TempData["ToastType"] = "error";
				return View(new List<VehicleResponsesDat>());
			}

			var residentId = userResult.Data.First().Id;

			// 🔹 Gọi API lấy danh sách phương tiện
			var vehicleUrl = $"{baseUrl}/api/Vehicles/filters?ResidentIds={residentId}";
			var vehicleResp = await _httpClient.GetAsync(vehicleUrl);

			if (!vehicleResp.IsSuccessStatusCode)
			{
				TempData["ToastMessage"] = "Không lấy được phương tiện.";
				TempData["ToastType"] = "error";
				return View(new List<VehicleResponsesDat>());
			}

			var vehicleJson = await vehicleResp.Content.ReadAsStringAsync();
			var vehicleResult = JsonConvert.DeserializeObject<ApiResult<List<VehicleResponsesDat>>>(vehicleJson);

			var vehicles = vehicleResult?.Data ?? new List<VehicleResponsesDat>();

			return View(vehicles);
		}


		[HttpPost]
		public async Task<IActionResult> UpdateVehicle([FromBody] VehicleResponseDat model)
		{
			try
			{
				var apiUrl = $"{GetBaseUrl.UrlQlpt?.TrimEnd('/')}/api/Vehicles/{model.Id}";

				var json = JsonConvert.SerializeObject(new
				{
					licensePlate = model.LicensePlate,
					type = model.Type
				});

				var content = new StringContent(json, Encoding.UTF8, "application/json");
				var response = await _httpClient.PutAsync(apiUrl, content);

				if (response.IsSuccessStatusCode)
					return Json(new { success = true });

				return StatusCode((int)response.StatusCode, new { success = false, message = "Update failed" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
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
					var ResdentBaseUrl = GetBaseUrl.UrlQlpt?.TrimEnd('/');
					var statusUrl = $"{payBaseUrl}/api/Payment/payos/status/{orderCode}";

					var statusResp = await _httpClient.GetAsync(statusUrl);
					if (statusResp.IsSuccessStatusCode)
					{
						var statusJson = await statusResp.Content.ReadAsStringAsync();
						var statusResult = JsonConvert.DeserializeObject<PayOSCreatePaymentResponse>(statusJson);

						string status = statusResult?.Data?.Status?.ToUpperInvariant();

						// ✅ Nếu thanh toán thành công → lấy invoice và update
						if (status == "PAID")
						{
							// 👉 Gọi API lấy chi tiết invoice để lấy ra BillId
							var getInvoiceUrl = $"{payBaseUrl}/api/Invoice/invoice-getById-{orderCode}"; // hoặc dùng /{invoiceId} nếu bạn có
							var invoiceResp = await _httpClient.GetAsync(getInvoiceUrl);

							if (invoiceResp.IsSuccessStatusCode)
							{
								var invoiceJson = await invoiceResp.Content.ReadAsStringAsync();
								var invoiceResult = JsonConvert.DeserializeObject<ApiResult<InvoiceResponseDat>>(invoiceJson);

								if (invoiceResult?.Success == true && invoiceResult.Data != null)
								{
									var invoiceData = invoiceResult.Data;

									// ✅ Tạo DTO thủ công để map dữ liệu, đảm bảo enum là int
									var updateInvoicePayload = new
									{
										id = invoiceData.Id,
										buildingID = invoiceData.BuildingID,
										roomID = invoiceData.RoomID,
										totalAmount = Convert.ToDecimal(invoiceData.TotalAmount),
										status = 1,
										dueDate = invoiceData.DueDate,
										billId = invoiceData.BillId
									};


									var updateInvoiceJson = JsonConvert.SerializeObject(updateInvoicePayload);
									var updateInvoiceContent = new StringContent(updateInvoiceJson, Encoding.UTF8, "application/json");

									var updateInvoiceUrl = $"{payBaseUrl}/api/Invoice/invoice-update";
									var updateInvoiceResp = await _httpClient.PutAsync(updateInvoiceUrl, updateInvoiceContent);

									if (!updateInvoiceResp.IsSuccessStatusCode)
									{
										TempData["ToastMessage"] += " ⚠️ Nhưng cập nhật trạng thái hóa đơn (Invoice) thất bại.";
										TempData["ToastType"] = "warning";
									}

									// 👉 Gọi API cập nhật trạng thái Bill
									var token = await GetAccessTokenAsync();

									var billId = invoiceData.BillId;
									var updateBillUrl = $"{ResdentBaseUrl}/api/Bills/UpdateStatus/{billId}";

									var request = new HttpRequestMessage(HttpMethod.Patch, updateBillUrl)
									{
										Content = new StringContent(JsonConvert.SerializeObject(2), Encoding.UTF8, "application/json")
									};

									// 👉 Gắn token vừa login được
									request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

									var updateResp = await _httpClient.SendAsync(request);

									if (!updateResp.IsSuccessStatusCode)
									{
										var respContent = await updateResp.Content.ReadAsStringAsync();
										Console.WriteLine($"Lỗi cập nhật trạng thái bill: {respContent}");

										TempData["ToastMessage"] += " ⚠️ Nhưng cập nhật trạng thái bill thất bại.";
										TempData["ToastType"] = "warning";
									}

								}



							}

						}

						// Hiển thị toast dù thành công hay không
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
				}
				catch (Exception ex)
				{
					TempData["ToastMessage"] = "Lỗi kiểm tra thanh toán: " + ex.Message;
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


		private async Task<string> GetAccessTokenAsync()
		{
			var loginPayload = new
			{
				userName = "thuetro",
				password = "123123"
			};

			using var loginClient = new HttpClient(); // Tạo HttpClient riêng để login
			var loginResponse = await loginClient.PostAsJsonAsync("http://103.149.252.12:8888/users/api/Auth/login", loginPayload);

			if (!loginResponse.IsSuccessStatusCode)
			{
				throw new Exception("Login failed.");
			}

			var result = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResultDat>>();
			return result.data.AccessToken;
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
					
						orderCode = orderCode,
						amount = (int)amount,
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