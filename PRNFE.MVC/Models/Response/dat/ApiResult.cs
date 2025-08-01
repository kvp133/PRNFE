﻿namespace PRNFE.MVC.Models.Response.dat
{
	public class ApiResult<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; }

		public T Data { get; set; }

		public List<string> Errors { get; set; }
		public int StatusCode { get; set; }
	}
}
