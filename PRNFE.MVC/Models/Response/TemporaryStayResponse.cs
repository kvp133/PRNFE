namespace PRNFE.MVC.Models.Response
{
    public class TemporaryStayResponse
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ResidentListResponses? Resident { get; set; }
        public enum TemporaryStayStatus
        {
            PendingApproval = 0, // Waiting for approval
            Active = 1,          // Approved and active
            Completed = 2,       // Completed stay
            Cancelled = 3,       // Cancelled by resident or admin
            Suspended = 4        // Suspended due to policy violation
        }
    }

    public class DetailTemporaryStayResponse
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ResidentListResponses? Resident { get; set; }

        public enum TemporaryStayStatus
        {
            PendingApproval = 0, // Waiting for approval
            Active = 1,          // Approved and active
            Completed = 2,       // Completed stay
            Cancelled = 3,       // Cancelled by resident or admin
            Suspended = 4        // Suspended due to policy violation
        }
    }
}
