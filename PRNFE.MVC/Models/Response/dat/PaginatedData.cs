namespace PRNFE.MVC.Models.Response.dat
{
	public class PaginatedData<T>
	{
		public List<T> Data { get; set; } = new();
		public int? TotalCount { get; set; }
		public int? PageNumber { get; set; }
		public int? PageSize { get; set; }
		public int? TotalPages { get; set; }
		public bool? HasPreviousPage { get; set; }
		public bool? HasNextPage { get; set; }
		public string? SortColumn { get; set; }
		public bool? IsDescending { get; set; }
	}

}
