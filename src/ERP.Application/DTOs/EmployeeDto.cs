using ERP.Domain.Enums;

namespace ERP.Application.DTOs
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime HireDate { get; set; }
        public EmployeeStatus Status { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public int LeaveBalance { get; set; }
        public int YearsOfService { get; set; }
        public int CurrentAllocation { get; set; }
        public bool IsOverallocated { get; set; }
        public List<EmployeeSkillDto> Skills { get; set; } = new();
        public List<string> PrimarySkills { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class EmployeeSkillDto
    {
        public Guid Id { get; set; }
        public string Technology { get; set; } = default!;
        public SkillLevel Level { get; set; }
        public int YearsExperience { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public string? Certification { get; set; }
    }
}