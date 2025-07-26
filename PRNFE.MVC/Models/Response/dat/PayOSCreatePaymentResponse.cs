using System.Text.Json.Serialization;

namespace PRNFE.MVC.Models.Response.dat
{
	public class PayOSCreatePaymentResponse
	{
		public string Code { get; set; }
		public string Desc { get; set; }
		public PayOSPaymentData Data { get; set; }
		public string Signature { get; set; }
	}

	public class PayOSPaymentData
	{
		// Chung cho cả 2 API
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("orderCode")]
		public int OrderCode { get; set; }

		[JsonPropertyName("amount")]
		public int Amount { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("createdAt")]
		public DateTime? CreatedAt { get; set; }

		[JsonPropertyName("canceledAt")]
		public DateTime? CanceledAt { get; set; }

		[JsonPropertyName("cancellationReason")]
		public string CancellationReason { get; set; }

		// ✅ Có ở API /status
		[JsonPropertyName("amountPaid")]
		public int? AmountPaid { get; set; }

		[JsonPropertyName("amountRemaining")]
		public int? AmountRemaining { get; set; }

		// ✅ Có ở API /create
		[JsonPropertyName("bin")]
		public string Bin { get; set; }

		[JsonPropertyName("accountNumber")]
		public string AccountNumber { get; set; }

		[JsonPropertyName("accountName")]
		public string AccountName { get; set; }

		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		[JsonPropertyName("paymentLinkId")]
		public string PaymentLinkId { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("expiredAt")]
		public long? ExpiredAt { get; set; }

		[JsonPropertyName("checkoutUrl")]
		public string CheckoutUrl { get; set; }

		[JsonPropertyName("qrCode")]
		public string QrCode { get; set; }
	}

}
