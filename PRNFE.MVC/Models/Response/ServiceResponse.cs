namespace PRNFE.MVC.Models.Response
{
    public class ServiceResponses
    {
     
            public Guid ServiceId { get; set; }
            public string ServiceName { get; set; } = string.Empty;
            public string Unit { get; set; } = string.Empty;
            public decimal PricePerUnit { get; set; }
            public bool IsMandatory { get; set; }
            public bool IsPerResident { get; set; }
            public DateTime CreatedAt { get; set; }
        
    }
}
