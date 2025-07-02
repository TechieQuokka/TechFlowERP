using ERP.Domain.Enums;

namespace ERP.Application.DTOs
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = default!;
        public Guid ManagerId { get; set; }
        public string ManagerName { get; set; } = default!;
        public ProjectStatus Status { get; set; }
        public ProjectType Type { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal ProfitMargin { get; set; }
        public decimal Progress { get; set; }
        public bool IsOverBudget { get; set; }
        public List<string> Technologies { get; set; } = new();
        public List<ProjectAssignmentDto> Assignments { get; set; } = new();
        public List<ProjectMilestoneDto> Milestones { get; set; } = new List<ProjectMilestoneDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProjectAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public int AllocationPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal HourlyRate { get; set; }
    }

    public class ProjectMilestoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal PaymentPercentage { get; set; }
        public MilestoneStatus Status { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysUntilDue { get; set; }
    }
}
