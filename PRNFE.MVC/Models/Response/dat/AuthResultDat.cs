namespace PRNFE.MVC.Models.Response.dat
{
	public class AuthResultDat
	{
		public bool Success { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string Error { get; set; }
	}
}
