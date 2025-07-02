namespace ERP.Application.DTOs
{
    public class TimeEntryDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = default!;
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = default!;
        public string ProjectCode { get; set; } = default!;
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string? TaskDescription { get; set; }
        public bool Billable { get; set; }
        public bool Approved { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public decimal EstimatedCost { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}