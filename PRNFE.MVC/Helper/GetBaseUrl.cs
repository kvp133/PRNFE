namespace PRNFE.MVC.Helper
{
	public static class GetBaseUrl
	{

		private static IConfiguration _configuration;

		public static void Configure(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public static string BaseUrl => _configuration?["ApiSettings:BaseUrl"];
		public static string UrlQlpt => _configuration?["ApiSettings:Url_qlpt"];
		public static string UrlPayment => _configuration?["ApiSettings:Url_payment"];
	}
}

